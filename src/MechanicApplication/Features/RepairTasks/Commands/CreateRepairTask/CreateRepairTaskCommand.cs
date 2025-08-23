using ErrorOr;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicDomain.RepairTasks.Enums;
using MediatR;

namespace MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;

public sealed record CreateRepairTaskCommand(
    string? Name, 
    decimal LaborCost,
    RepairDurationInMinutes? EstimatedDurationInMins, 
    List<CreateRepairTaskPaskPartCommand> Parts
    ) : IRequest<ErrorOr<RepairTaskDTO>>;

