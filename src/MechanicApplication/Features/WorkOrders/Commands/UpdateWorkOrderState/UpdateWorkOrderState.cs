
using ErrorOr;
using MediatR;
using MechanicDomain.WorkOrders.Enums;
namespace MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed record UpdateWorkOrderState(Guid WorkOrderId, WorkOrderState State) : IRequest<ErrorOr<Updated>>;
