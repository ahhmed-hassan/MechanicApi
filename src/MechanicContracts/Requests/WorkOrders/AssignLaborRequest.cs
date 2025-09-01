using System.ComponentModel.DataAnnotations;

namespace MechanicContracts.Requests.WorkOrders;

public class AssignLaborRequest
{
    [Required(ErrorMessage = "LaborID is required")]
    public string LaborId { get; set;  } = string.Empty; 
}
