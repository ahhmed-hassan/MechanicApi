using ErrorOr;
using MechanicApplication.Features.RepairTasks.DTOs;
using MediatR;

namespace MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;

public sealed record CreateRepairTaskPaskPartCommand(
    string Name, 
    decimal Cost, 
    int Quantity
    )
    :IRequest<ErrorOr<Success>>
{
}