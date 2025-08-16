using ErrorOr;
using MediatR;
using System.Collections.Immutable;


namespace MechanicApplication.Features.Customers.Commands;

public sealed record UpdateCustomerCommand(
 Guid CustomerId,
 string Name,
 string PhoneNumber,
 string Email,
 ImmutableList<UpdateVehicleCommand> Vehicles

) : IRequest<ErrorOr<Updated>>;
