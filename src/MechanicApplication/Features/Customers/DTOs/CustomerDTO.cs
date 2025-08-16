using System.Collections.Immutable;

namespace MechanicApplication.Features.Customers.DTOs;

public record CustomerDto(
    Guid CustomerId,
    string Name,
    string PhoneNumber,
    string Email,
    ImmutableList<VehicleDto> Vehicles)
{
    public virtual bool Equals(CustomerDto? other)
        => other is not null && CustomerId == other.CustomerId;

    public override int GetHashCode()
        => CustomerId.GetHashCode();
}