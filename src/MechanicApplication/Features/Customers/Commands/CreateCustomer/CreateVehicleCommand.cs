using ErrorOr;
using MechanicApplication.Features.Customers.DTOs;
using MediatR;

namespace MechanicApplication.Features.Customers.Commands.CreateCustomer;

public sealed record CreateVehicleCommand(
 string Make,
 string Model,
 int Year,
 string LicensePlate) : IRequest<ErrorOr<VehicleDTO>>;