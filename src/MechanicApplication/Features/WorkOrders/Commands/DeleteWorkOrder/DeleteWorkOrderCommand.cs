using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed record class DeleteWorkOrderCommand(Guid workOrderId) 
    : IRequest<ErrorOr<Deleted>>;
