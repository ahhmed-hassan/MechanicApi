using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.Customers.Commands;

public sealed record UpdateVehicleCommand(
 Guid? VehicleId,
 string Make,
 string Model,
 int Year,
 string LicensePlate) : IRequest<ErrorOr<Updated>>;