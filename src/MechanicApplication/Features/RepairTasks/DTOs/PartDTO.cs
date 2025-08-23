namespace MechanicApplication.Features.RepairTasks.DTOs;

public sealed record PartDTO(
    Guid Id, 
    decimal Cost,
    int Quantity,
    string Name = ""
    );
