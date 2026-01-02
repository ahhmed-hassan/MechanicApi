using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWrokOrderCommandHandler
    (
    )
    : IRequestHandler<DeleteWorkOrderCommand, ErrorOr<Deleted>>
{
    public Task<ErrorOr<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
