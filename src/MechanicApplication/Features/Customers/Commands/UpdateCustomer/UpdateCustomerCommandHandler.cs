

using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(
    ILogger<UpdateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<UpdateCustomerCommand, ErrorOr<Updated>>
{
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IAppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    public async Task<ErrorOr<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(navigationPropertyPath: rt => rt.Vehicles)
            .FirstOrDefaultAsync(rt => rt.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for update.", request.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }
        List<ErrorOr<Vehicle>> vehicles = request.Vehicles.Select(v =>
            Vehicle.Create(v.VehicleId ?? Guid.NewGuid(), v.Make, v.Model, v.Year, v.LicensePlate)).ToList();
        if (vehicles.Where(v => v.IsError).SelectMany(e => e.Errors).ToList() is { Count: > 0 } errors)
        {
            _logger.LogWarning("Customer update aborted. Vehicles contain errors.");
            return errors;
        }
        var updateCustomerResult = customer.Update(request.Name, request.Email, request.PhoneNumber);

        return updateCustomerResult.Then(_ =>
        customer.UpsertParts(vehicles.ConvertAll(v => v.Value))).ThenDo(async _ =>
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.RemoveByTagAsync("customer", cancellationToken);
            }).Then(_ => Result.Updated);

    }
}
