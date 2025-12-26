using MechanicApplication.Features.Customers.DTOs;
using MechanicApplication.Features.Labors.DTOs;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicApplication.Features.WorkOrders.Dtos;

public class WorkOrderDTO
{
    public Guid WorkOrderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Spot Spot { get; set; }
    public VehicleDTO? Vehicle { get; set; }
    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }
    public List<RepairTaskDTO> RepairTasks { get; set; } = [];
    public LaborDTO? Labor { get; set; }
    public WorkOrderState State { get; set; }
    public decimal TotalPartCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public decimal TotalCost { get; set; }
    public int TotalDurationInMins { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}