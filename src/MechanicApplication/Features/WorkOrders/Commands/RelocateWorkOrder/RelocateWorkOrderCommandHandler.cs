
using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder
{
    public class RelocateWorkOrderCommandHandler(IAppDbContext context) : IRequestHandler<RelocateWorkOrderCommand, ErrorOr<Updated>>

    {
        public Task<ErrorOr<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
