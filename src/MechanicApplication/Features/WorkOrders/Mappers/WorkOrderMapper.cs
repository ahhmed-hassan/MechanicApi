using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.Labors.DTOs;
using MechanicApplication.Features.RepairTasks.Mappers;
using MechanicApplication.Features.WorkOrders.Dtos;
using MechanicDomain.WorkOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicApplication.Features.WorkOrders.Mappers;
public static class WorkOrderMapper
{
    public static WorkOrderDTO ToDto(this WorkOrder entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new WorkOrderDTO
        {
            WorkOrderId = entity.Id,
            Spot = entity.Spot,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            Labor = entity.Labor is null ? null : new LaborDTO
            {
                LaborId = entity.LaborId,
                Name = $"{entity.Labor.FirstName} {entity.Labor.LastName}"
            },
            RepairTasks = entity.RepairTasks.ToDtos(),
            Vehicle = entity.Vehicle is null ? null : entity.Vehicle.ToDto(),
            State = entity.State,
            TotalPartCost = entity.RepairTasks.SelectMany(t => t.Parts).Sum(p => p.Cost * p.Quantity),
            TotalLaborCost = entity.RepairTasks.Sum(p => p.LaborCost),
            TotalCost = entity.RepairTasks.Sum(rt => rt.TotalCost),
            TotalDurationInMins = entity.RepairTasks.Sum(rt => (int)rt.EstimatedDurationInMins),
            InvoiceId = entity.Invoice?.Id,
            CreatedAt = entity.CreatedAtUtc
        };
    }

    public static List<WorkOrderDTO> ToDtos(this IEnumerable<WorkOrder> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }
}