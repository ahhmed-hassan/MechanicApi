using ErrorOr;
using MechanicDomain.Customers;
using MechanicDomain.Customers.Vehicles;

namespace MechanicShop.Tests.Common.Customers;

public static class CustomerFactory
{
    public static ErrorOr<Customer> CreateCustomer(Guid? id = null,
                                                   string name = "Customer #1",
                                                   string phoneNumber = "5555555555",
                                                   string email = "customer01@localhost",
                                                   List<Vehicle>? vehicles = null) => Customer.Create(
            id ?? Guid.NewGuid(),
            name,
            phoneNumber,
            email,
            vehicles ?? [VehicleFactory.CreateVehicle().Value, VehicleFactory.CreateVehicle().Value]);
}