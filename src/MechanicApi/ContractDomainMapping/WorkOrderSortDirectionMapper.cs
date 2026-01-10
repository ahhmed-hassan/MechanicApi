namespace MechanicApi.ContractDomainMapping;
using MechanicContracts.Requests.WorkOrders;
public static class ContractsWorkOrderColumnToApplicationWorkOrderColumn
{
    public static MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn ToApplicationWorkOrderColumn(this WorkOrderSortColumn column) 
        => column switch
    {
        WorkOrderSortColumn.CreatedAt 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.CreatedAt,
        WorkOrderSortColumn.UpdatedAt
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.UpdatedAt,
        WorkOrderSortColumn.StartAt 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.StartAt,
        WorkOrderSortColumn.EndAt 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.EndAt,
        WorkOrderSortColumn.State 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.State,
        WorkOrderSortColumn.Spot 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.Spot,
        WorkOrderSortColumn.Total 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.Total,
        WorkOrderSortColumn.VehicleId 
            => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.VehicleId,
        WorkOrderSortColumn.LaborId => MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums.WorkOrderSortColumn.LaborId,
        _ => throw new ArgumentOutOfRangeException(nameof(column), $"Not expected work order sort column value: {column}"),
    };
}
