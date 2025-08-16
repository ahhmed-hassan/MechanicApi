
using MechanicDomain.Customers;

namespace MechanicApplication.Features.Customers.DTOs;

public record VehicleDTO(Guid VehicleId, string Make, string Model, int Year, string LicensePlate)
{
    public virtual bool Equals(VehicleDTO? other)
       => other is not null && VehicleId == other.VehicleId;

    public override int GetHashCode()
        => VehicleId.GetHashCode();
};
