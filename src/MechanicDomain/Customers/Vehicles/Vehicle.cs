
using ErrorOr;
using MechanicDomain.Abstractions;

namespace MechanicDomain.Customers.Vehicles;

public sealed class Vehicle : AuditableEntity
{
    public Guid CustomerId { get; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string LicensePlate { get; private set; } = string.Empty;
    public Customer? Customer { get; set; }
    public static int FirstYear => 1886;

    public string VehicleInfo => $"{Make} | {Model} | {Year}";

    private Vehicle(Guid id, string make, string model, int year, string licensePlate)
       : base(id)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    private Vehicle() { }

    private static ErrorOr<Guid> checkValid(Guid id, string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make)) return VehicleErrors.MakeRequired;
        if (string.IsNullOrWhiteSpace(model)) return VehicleErrors.ModelRequired;
        if (string.IsNullOrWhiteSpace(licensePlate)) return VehicleErrors.LicensePlateRequired;
        if (year < 5 || year > DateTime.UtcNow.Year) return VehicleErrors.YearInvalid;
        return id;
    }

    public static ErrorOr<Vehicle> Create(Guid id, string make, string model, int year, string licensePlate)
    {
        return checkValid(id, make, model, year, licensePlate)
            .Then(id => new Vehicle(id, make, model, year, licensePlate));

    }
    public ErrorOr<Updated> Update(string make, string model, int year, string licensePlate)
    {
        return checkValid(Id, make, model, year, licensePlate).
            ThenDo(_ =>
       { Make = make; Model = model; Year = year; LicensePlate = licensePlate; }
       ).Then(_ => Result.Updated);
    }
}

