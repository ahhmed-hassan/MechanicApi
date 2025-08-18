using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.Customers.DTOs;
using MechanicApplication.Features.Customers.Queries.GetCustoemerByID;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(
    ILogger<GetCustomerByIdQueryHandler> logger,
    IAppDbContext context
    )
    : IRequestHandler<GetCustomerByIdQuery, ErrorOr<CustomerDTO>>
{
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    public async Task<ErrorOr<CustomerDTO>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CustomerId, cancellationToken);
        if (customer is not null)
        {
            _logger.LogInformation("Customer with ID {CustomerId} retrieved successfully.", query.CustomerId);
            return customer.ToDto();
        }
        _logger.LogWarning("Customer with ID {CustomerId} not found.", query.CustomerId);
        return Error.NotFound(
            code: "Customer.NotFound",
            description: $"Customer with ID {query.CustomerId} not found."
        );
    }
}

