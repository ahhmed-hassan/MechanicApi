
using MechanicContracts.Shared;

namespace MechanicContracts.Requests.WorkOrders;

public sealed class WorkOrderFilterRequest
{
    public WorkOrderState? State { get; set; }
    public Guid? VehicleId { get; set; } = null;
    public Guid? LaborId { get; set; } = null;
    public DateTime? StartDateFrom { get; set; } = null;
    public DateTime? StartDateTo { get; set; } = null;
    public DateTime? EndDateFrom { get; set; }  = null;
    public DateTime? EndDateTo { get; set; } = null;
    public Spot? Spot { get; set; } = null;
    public WorkOrderSortColumn SortColumn { get; set; } = WorkOrderSortColumn.CreatedAt;
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public string? SearchTerm { get; set; } = null;
}
public enum WorkOrderSortColumn
{
    CreatedAt,
    UpdatedAt,
    StartAt,
    EndAt,
    State,
    Spot,
    Total,
    VehicleId,
    LaborId
}

public enum SortDirection
{
    Asc,
    Desc
}