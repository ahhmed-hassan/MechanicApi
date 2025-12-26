

using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler
    : IRequestHandler<CreateWorkOrderCommand, ErrorOr<Dtos.WorkOrderDTO>>
{
    public async Task<ErrorOr<Dtos.WorkOrderDTO>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
