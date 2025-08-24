namespace MechanicApplication.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed record UpdateRepairTaskPartCommand(
 Guid? PartId,
 string Name,
 decimal Cost,
 int Quantity
    );