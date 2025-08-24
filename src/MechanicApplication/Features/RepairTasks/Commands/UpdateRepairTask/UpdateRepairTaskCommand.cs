using ErrorOr;
using MechanicDomain.RepairTasks.Enums;
using MediatR;

namespace MechanicApplication.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed record UpdateRepairTaskCommand(
    Guid RepairTaskId, 
    string Name,
    decimal LaborCost,
    RepairDurationInMinutes EstimatedDurationInMins, 
    List<UpdateRepairTaskPartCommand> CommandParts

    ) : IRequest<ErrorOr<Updated>>
{
}