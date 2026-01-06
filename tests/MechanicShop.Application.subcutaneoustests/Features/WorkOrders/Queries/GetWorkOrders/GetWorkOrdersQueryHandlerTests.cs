
using MechanicApi.Application.subcutaneoustests.Common;
using MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders;

using MechanicDomain.RepairTasks.Enums;
using MechanicDomain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

[Collection(WebbAppFactoryColllection.CollectionName)]
public class GetWorkOrdersQueryHandlerTests(WebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Handle_WithBasicPagination_ShouldReturnFirstPage()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.SaveChangesAsync(default);

        // Create 3 work orders
        var baseTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime,
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime.AddHours(2),
            laborId: employee.Id,
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        var workOrder3 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime.AddHours(4),
            laborId: employee.Id,
            spot: Spot.C,
            repairTasks: [repairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder1);
        await _dbContext.WorkOrders.AddAsync(workOrder2);
        await _dbContext.WorkOrders.AddAsync(workOrder3);
        await _dbContext.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 2,
            PageSize: 2);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);

        var paginatedList = result.Value;
        Assert.Equal(3, paginatedList.TotalCount);
        Assert.Equal(2, paginatedList.TotalPages);
        Assert.Equal(2, paginatedList.PageNumber);
        Assert.Equal(2, paginatedList.PageSize);
        Assert.NotNull(paginatedList.Items);
        Assert.Equal(1, paginatedList.Items?.Count);

        // Verify items contain expected data
        Assert.All(paginatedList.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.WorkOrderId);
            Assert.NotNull(item.Vehicle);
            Assert.Equal(vehicle.Id, item.Vehicle.VehicleId);
            Assert.Equal(customer.Name, item.Customer);
            Assert.NotNull(item.Labor);
            Assert.Single(item.RepairTasks);
        });
        }
}