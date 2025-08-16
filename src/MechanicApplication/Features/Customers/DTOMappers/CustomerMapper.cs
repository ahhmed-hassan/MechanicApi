using MechanicApplication.Features.Customers.DTOs;
using MechanicDomain.Customers;
using MechanicDomain.Customers.Vehicles;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicApplication.Features.Customers.DTOMappers;

public static class CustomerMapper
{
    public static CustomerDTO ToDto(this Customer entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new CustomerDTO
        (
            CustomerId : entity.Id,
            Name: entity.Name!,
            PhoneNumber: entity.PhoneNumber!,
            Email: entity.Email!,
            Vehicles: entity.Vehicles?.Select(v => v.ToDto()).ToImmutableList() ?? []
        );
    }

    public static List<CustomerDTO> ToDtos(this IEnumerable<Customer> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }

    public static VehicleDTO ToDto(this Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new VehicleDTO(entity.Id, entity.Make!, entity.Model!, entity.Year, entity.LicensePlate!);
    }

    public static List<VehicleDTO> ToDtos(this IEnumerable<Vehicle> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }
}
