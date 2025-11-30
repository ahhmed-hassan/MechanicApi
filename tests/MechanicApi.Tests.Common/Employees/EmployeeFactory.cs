using ErrorOr;
using MechanicDomain.Employees;
using MechanicDomain.Identity;

namespace MechanicShop.Tests.Common.Employees;

public static class EmployeeFactory
{
    public static ErrorOr<Employee> CreateEmployee(
        Guid? id = null,
        string firstName = "John",
        string lastName = "Doe",
        Role role = Role.Labor)
    {
        return Employee.Create(
            id ?? Guid.NewGuid(),
            firstName,
            lastName,
            role);
    }

    public static ErrorOr<Employee> CreateLabor(Guid? id = null, string firstName = "John", string lastName = "Labor")
    {
        return CreateEmployee(
            id,
            firstName,
            lastName,
            Role.Labor);
    }

    public static ErrorOr<Employee> CreateManager(Guid? id = null, string firstName = "John", string lastName = "Manager")
    {
        return CreateEmployee(
            id,
            firstName,
            lastName,
            Role.Manager);
    }
}