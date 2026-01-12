using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.Customers.DTOs;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicApplication.Features.WorkOrders.Dtos;

public class WorkOrderListItemDTO
{
    public Guid WorkOrderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public VehicleDTO? Vehicle { get; set; }
    public string? Customer { get; set; }
    public string? Labor { get; set; }
    public WorkOrderState State {get; set;}
    public Spot Spot { get; set; }
    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }
    public List<string> RepairTasks { get; set; } = new();

    public WorkOrderListItemDTO(WorkOrder wo)
    {
        WorkOrderId = wo.Id;
        InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id;
        Spot = wo.Spot;
        StartAtUtc = wo.StartAtUtc;
        EndAtUtc = wo.EndAtUtc;
        Vehicle = wo.Vehicle!.ToDto();
        Customer = wo.Vehicle!.Customer!.Name;
        Labor = wo.Labor != null
          ? wo.Labor.FirstName + " " + wo.Labor.LastName
          : null;
        State = wo.State;
        RepairTasks = wo.RepairTasks.Select(rt => rt.Name).ToList();
    }

}
