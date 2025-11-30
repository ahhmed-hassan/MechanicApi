using MechanicDomain.Customers.Vehicles;
using ErrorOr;

namespace MechanicShop.Tests.Common.Customers;

public static class VehicleFactory
{
    public static ErrorOr<Vehicle> CreateVehicle(Guid? id = null,
                                                 string make = "Honda",
                                                 string model = "Accord",
                                                 int year = 2024,
                                                 string licensePlate = "ABC 123") =>
        Vehicle.Create(id ?? Guid.NewGuid(), make, model, year, licensePlate);
}