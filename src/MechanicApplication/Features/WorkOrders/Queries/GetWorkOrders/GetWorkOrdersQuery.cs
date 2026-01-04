
using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Common.Models;
using MechanicApplication.Features.WorkOrders.Dtos;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders;

public sealed record  GetWorkOrdersQuery
    (
    int Page, 
    int PageSize
    )
    : ICachedQuery<ErrorOr<PageinatedList<WorkOrderListItemDTO>>>
{
    public string CacheKey =>
        $"work-orders:p={Page}:ps={PageSize}"
        ;

    public string[] Tags => [Constants.Cache.WorkOrders.Single]
    ;

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

};

internal static class StringExtensions
{
    internal static string OrHyphen(this string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;
}
