using ErrorOr;
using MechanicApplication.Common.Models;
using MechanicApplication.Features.WorkOrders.Dtos;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, ErrorOr<PageinatedList<WorkOrderListItemDTO>>>
{
    public Task<ErrorOr<PageinatedList<WorkOrderListItemDTO>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<PageinatedList<WorkOrderListItemDTO>>>(
          Error.Failure(
              code: "WorkOrders.Failed",
              description: "Something went wrong.")
      );
    }
}
