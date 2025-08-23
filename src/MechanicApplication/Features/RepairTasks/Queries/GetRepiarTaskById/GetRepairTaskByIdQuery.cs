
using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.RepairTasks.DTOs;
using MediatR;

namespace MechanicApplication.Features.RepairTasks.Queries.GetRepiarTaskById;

public sealed record GetRepairTaskByIdQuery(Guid RepairTaskId):IRequest<ErrorOr<RepairTaskDTO>>;
