namespace MechanicApplication.Features.RepairTasks.DTOs;

public sealed record PartDTO(
    Guid ID, 
    decimal Cost,
    int Quantity,
    string Name = ""
    );
