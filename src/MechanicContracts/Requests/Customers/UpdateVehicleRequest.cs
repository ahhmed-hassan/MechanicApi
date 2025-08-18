using System.ComponentModel.DataAnnotations;

namespace MechanicContracts.Requests.Customers;

public sealed class UpdateVehicleRequest
{
    public Guid? VehicleId { get; set; }

    [Required(ErrorMessage = "Make is required.")]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required.")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Year is required.")]
    [Range(minimum: 1886, maximum: int.MaxValue,  ErrorMessage = "Year must be a valid year (1886 or later).")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Spot is required.")]
    public string LicensePlate { get; set; } = string.Empty;
}
