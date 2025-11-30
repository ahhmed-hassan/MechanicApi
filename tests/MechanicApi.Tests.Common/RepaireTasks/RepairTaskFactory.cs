using System.Collections.Generic;
using ErrorOr;
using MechanicDomain.RepairTasks;
using MechanicDomain.RepairTasks.Enums;
using MechanicDomain.RepairTasks.Parts;

namespace MechanicShop.Tests.Common.RepaireTasks;

public static class RepairTaskFactory
{
    public static ErrorOr<RepairTask> CreateRepairTask(
        Guid? id = null,
        string name = "Brake Inspection",
        decimal laborCost = 100m,
        RepairDurationInMinutes repairDurationInMinutes = RepairDurationInMinutes.Min30,
        List<Part>? parts = null)
    {
        return RepairTask.Create(
            id ?? Guid.NewGuid(),
            name,
            laborCost,
            repairDurationInMinutes,
            parts ?? new List<Part> { Part.Create(Guid.NewGuid(), "Brake Pads", 50m, 1).Value });
    }
}