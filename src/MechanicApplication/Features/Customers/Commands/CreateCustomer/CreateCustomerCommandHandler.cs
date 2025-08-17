

using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.Customers.DTOs;
using MechanicDomain.Customers;
using MechanicDomain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler
    (ILogger<CreateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    )
    : IRequestHandler<CreateCustomerCommand, ErrorOr<CustomerDTO>>
{
    private readonly ILogger<CreateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<CustomerDTO>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Customers
            .AnyAsync(c => c.Email!.Trim().ToLowerInvariant() == request.Email.Trim().ToLowerInvariant()
            , cancellationToken);

        if(emailExists)
        {
            _logger.LogWarning("Customer creation aborted. Email {Email} already exists.", request.Email);
            return CustomerErrors.CustomerExists; 
        }
        
        List<ErrorOr<Vehicle>> vehicles = request.Vehicles.Select(v =>
        Vehicle.Create(Guid.NewGuid(), v.Make, v.Model, v.Year, v.LicensePlate)
        ).ToList(); 
        
        if(vehicles.Where(v => v.IsError).SelectMany(e => e.Errors).ToList() is { Count : > 0 } errors)
        {
            _logger.LogWarning("Customer creation aborted. Vehicles contain errors.");
            return errors; 
        }

        
       var createdCustomer = Customer.Create(
           Guid.NewGuid(),
           request.Name,
           request.PhoneNumber,
           request.Email,
           vehicles.ConvertAll(v => v.Value));
        
        return createdCustomer.ThenDo(
            async customer => 
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.RemoveByTagAsync(Constants.Cache.Customers.Single, cancellationToken);
                _logger.LogInformation("Customer created successfully by Id : {CustomerId}.", customer.Id);
               
            }
            
        ).Then(c => c.ToDto());
    }
}
