using MechanicDomain.RepairTasks.Enums;

namespace MechanicApplication.Features.RepairTasks.DTOs;

public sealed class RepairTaskDTO
{
    public Guid RepairTaskId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RepairDurationInMinutes EstimatedDurationInMins { get; set; }
    public decimal LaborCost { get; set; }
    public decimal TotalCost { get; set; }
    public List<PartDTO> Parts { get; set; } = [];
}
    

