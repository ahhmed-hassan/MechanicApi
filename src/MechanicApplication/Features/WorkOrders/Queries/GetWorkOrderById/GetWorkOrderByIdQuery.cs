using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.WorkOrders.Dtos;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrderById;

public record GetWorkOrderByIdQuery(Guid id)
    : ICachedQuery<ErrorOr<WorkOrderDTO>>
{
    public string CacheKey => Constants.Cache.WorkOrders.Single;

    public string[] Tags => [Constants.Cache.WorkOrders.Plural];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
