

using MechanicContracts.Shared;

using System.ComponentModel.DataAnnotations;

namespace MechanicContracts.Requests.RepairTasks;

public class CreateRepairTaskRequest
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "LaborCost is required")]
    [Range(0, 1000, ErrorMessage = "LaborCost must be a non-negative value less than 10,000")]
    public decimal LaborCost { get; set; }
    [Required(ErrorMessage = "RepairDurationInMinutes is required")]
    public RepairDurationInMinutes? EstimatedDurationInMins { get; set; }
    [Required(ErrorMessage = "Parts are required")]
    [MinLength(1, ErrorMessage = "At least one part is required")]
    //TODO: Custom validation attribute to validate each part in the list
    //[ValidateComplexType]
    public List<CreateRepairTaskPartRequest> Parts { get; set; } = new List<CreateRepairTaskPartRequest>();
}
