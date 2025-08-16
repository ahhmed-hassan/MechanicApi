

using ErrorOr;
using MechanicApplication.Features.Customers.DTOs;
using MediatR;

namespace MechanicApplication.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string PhoneNumber,
    string Email,
    List<CreateVehicleCommand> Vehicles

) : IRequest<ErrorOr<CustomerDTO>>;
