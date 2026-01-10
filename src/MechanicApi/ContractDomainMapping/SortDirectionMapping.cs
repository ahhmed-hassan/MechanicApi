using MechanicContracts.Requests.WorkOrders;

namespace MechanicApi.ContractDomainMapping;

public static class SortDirectionMapping
{
    public static MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.SortDirection ToApplicationSortDirection(this SortDirection direction) 
        => direction switch
    {
        SortDirection.Asc 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.SortDirection.Asc,
        SortDirection.Desc
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.SortDirection.Desc,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Not expected sort direction value: {direction}"),
    };
}
