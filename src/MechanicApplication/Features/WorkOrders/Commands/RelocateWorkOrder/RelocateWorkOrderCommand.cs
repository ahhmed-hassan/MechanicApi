using ErrorOr;
using MechanicDomain.WorkOrders.Enums;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder
{
    public sealed record RelocateWorkOrderCommand(
        Guid Id, 
        DateTimeOffset NewStartAt, 
        Spot NewSpot
        ) : IRequest<ErrorOr<Updated>>;
    
}