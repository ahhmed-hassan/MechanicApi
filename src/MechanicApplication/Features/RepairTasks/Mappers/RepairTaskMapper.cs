
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicDomain.RepairTasks;
using MechanicDomain.RepairTasks.Parts;
using System.Collections.Immutable;

namespace MechanicApplication.Features.RepairTasks.Mappers;
public static class RepairTaskMapper
{
    public static RepairTaskDTO ToDto(this RepairTask entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new RepairTaskDTO
        {
            RepairTaskId = entity.Id,
            Name = entity.Name!,
            LaborCost = entity.LaborCost,
            TotalCost = entity.TotalCost,
            EstimatedDurationInMins = entity.EstimatedDurationInMins,
            Parts = entity.Parts.ToList().ConvertAll(ToDto)
        };
    }

    public static List<RepairTaskDTO> ToDtos(this IEnumerable<RepairTask> entities)
    {
        return  entities.Select(e => e.ToDto()).ToList();
    }

    public static PartDTO ToDto(this Part entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PartDTO
        (
            Id : entity.Id,
            Name : entity.Name!,
            Cost : entity.Cost,
            Quantity: entity.Quantity
        );
    }

    public static List<PartDTO> ToDtos(this IEnumerable<Part> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}

