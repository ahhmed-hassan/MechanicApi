

using MechanicContracts.Shared;
using System.ComponentModel.DataAnnotations;

namespace MechanicContracts.Requests.WorkOrders;

public sealed class CreateWorkOrderRequest
{
   

    [Required(ErrorMessage ="Spot is Required")]
    [Range(0,3, ErrorMessage ="The Range is [0,3]")]
    public Spot Spot { get; set; }

    [Required(ErrorMessage = "VehicleId is required")]
    public Guid VehicleId { get; set; }

    [Required(ErrorMessage = "LaborIs is required")]
    public Guid LaborId { get; set;  }

    [MinLength(1, ErrorMessage = "Work Order should refernce at least one specific repair task")]
    public List<Guid> RepairTasksIds { get; set; } = [];

    [Required(ErrorMessage = "Start time is required")]
    public DateTimeOffset StartAtUtc { get; set; }

}
