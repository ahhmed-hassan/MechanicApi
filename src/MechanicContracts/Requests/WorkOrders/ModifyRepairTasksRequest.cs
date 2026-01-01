using System.ComponentModel.DataAnnotations;

namespace MechanicContracts.Requests.WorkOrders;

public sealed class ModifyRepairTasksRequest
{
    [MinLength(1, ErrorMessage = "At least one part is required")]
    public Guid[] RepairTasksIds { get; set; } = []; 
}
