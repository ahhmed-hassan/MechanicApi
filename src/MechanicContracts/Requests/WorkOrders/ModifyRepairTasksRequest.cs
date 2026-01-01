namespace MechanicContracts.Requests.WorkOrders;

public sealed class ModifyRepairTasksRequest
{
    public Guid[] RepairTasksIds { get; set; } = []; 
}
