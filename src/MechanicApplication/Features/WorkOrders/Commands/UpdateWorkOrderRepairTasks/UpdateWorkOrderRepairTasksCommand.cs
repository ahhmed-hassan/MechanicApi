
using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed record UpdateWorkOrderRepairTasksCommand(
    Guid WorkOrderId, Guid[] RepairTasksIds
    ) : IRequest<ErrorOr<Updated>>;

