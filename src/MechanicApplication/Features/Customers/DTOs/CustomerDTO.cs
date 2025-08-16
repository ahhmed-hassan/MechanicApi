using System.Collections.Immutable;

namespace MechanicApplication.Features.Customers.DTOs;

public record CustomerDTO(
    Guid CustomerId,
    string Name,
    string PhoneNumber,
    string Email,
    ImmutableList<VehicleDTO> Vehicles)
{
    public virtual bool Equals(CustomerDTO? other)
        => other is not null && CustomerId == other.CustomerId;

    public override int GetHashCode()
        => CustomerId.GetHashCode();
}