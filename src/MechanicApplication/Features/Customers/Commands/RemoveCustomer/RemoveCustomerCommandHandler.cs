using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.Customers.Commands.RemoveCustomer;

public sealed class RemoveCustomerCommandHandler(
    ILogger<RemoveCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    ) : IRequestHandler<RemoveCustomerCommand, ErrorOr<Deleted>>
{
    private readonly ILogger<RemoveCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<Deleted>> Handle(RemoveCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check if the customer exists

        var customer = _context.Customers.Find(request.CustomerId);
        if (customer is null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found.", request.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }
        //Check if it has any work orders
        var hasAssociatedWorkOrders = await _context.WorkOrders
             .AnyAsync(wo => wo.Vehicle != null && wo.Vehicle.CustomerId == request.CustomerId, cancellationToken); 

          
        if(hasAssociatedWorkOrders)
        {
            _logger.LogWarning("Customer with ID {CustomerId} has associated work orders (scheduled, or in-progress) and cannot be deleted.", request.CustomerId);
        }
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync("customer", cancellationToken);
        _logger.LogInformation("Customer with ID {CustomerId} has been removed successfully.", request.CustomerId);
        return Result.Deleted;

    }
}
