
using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Common.Models;
using MechanicApplication.Features.WorkOrders.Dtos;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders;

public sealed record  GetWorkOrdersQuery
    (
    int Page, 
    int PageSize, 
    MechanicDomain.WorkOrders.Enums.WorkOrderState? State = null,
    Guid? VehicleId = null,
    Guid? LaborId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? EndDateFrom = null,
    DateTime? EndDateTo = null,
    MechanicDomain.WorkOrders.Enums.Spot? Spot = null)
    : ICachedQuery<ErrorOr<PaginatedList<WorkOrderListItemDTO>>>
{
    public string CacheKey =>
        $"work-orders:p={Page}:ps={PageSize}" + 
        $":s={State?.ToString().OrHyphen()}"
        + $":v={VehicleId?.ToString().OrHyphen()}"
        + $":l={LaborId?.ToString().OrHyphen()}"
        + $":sdf={StartDateFrom?.ToString("yyyyMMdd").OrHyphen()}"
        + $":sdt={StartDateTo?.ToString("yyyyMMdd").OrHyphen()}"
        + $":edf={EndDateFrom?.ToString("yyyyMMdd").OrHyphen()}"
        + $":edt={EndDateTo?.ToString("yyyyMMdd").OrHyphen()}"
        + $":sp={Spot?.ToString().OrHyphen()}"

        ;

    public string[] Tags => [Constants.Cache.WorkOrders.Single]
    ;

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

};

internal static class StringExtensions
{
    internal static string OrHyphen(this string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;
}
