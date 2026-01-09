
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
    public async Task Handle_WithBasicPagination_ShouldReturnSecondPage()
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

    [Fact]
    public async Task Handle_WithStateFilter_ShouldReturnOnlyMatchingState()
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

        var baseTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        // Create work orders with different states
        var scheduledWorkOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime,
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;
        // scheduledWorkOrder is already in Scheduled state (default)

        var inProgressWorkOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime.AddHours(2),
            laborId: employee.Id,
            spot: Spot.B,
            repairTasks: [repairTask]).Value;
        inProgressWorkOrder.UpdateState(WorkOrderState.InProgress); // Transition to InProgress

        var completedWorkOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime.AddHours(4),
            laborId: employee.Id,
            spot: Spot.C,
            repairTasks: [repairTask]).Value;
        completedWorkOrder.UpdateState(WorkOrderState.InProgress);
        completedWorkOrder.UpdateState(WorkOrderState.Completed); // Transition to Completed

        await _dbContext.WorkOrders.AddAsync(scheduledWorkOrder);
        await _dbContext.WorkOrders.AddAsync(inProgressWorkOrder);
        await _dbContext.WorkOrders.AddAsync(completedWorkOrder);
        await _dbContext.SaveChangesAsync(default);

        // Query only for InProgress work orders
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            State: WorkOrderState.InProgress);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);

        var paginatedList = result.Value;
        Assert.Equal(1, paginatedList.TotalCount); // Only 1 InProgress work order
        Assert.Single(paginatedList.Items);

        var item = paginatedList.Items.First();
        Assert.Equal(inProgressWorkOrder.Id, item.WorkOrderId);
        Assert.Equal(WorkOrderState.InProgress, item.State);
    }

    [Fact]
    public async Task Handle_WithVehicleFilter_ShouldReturnOnlyMatchingVehicle()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        // Create two vehicles for two different customers
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer1 = CustomerFactory.CreateCustomer(vehicles: [vehicle1]).Value;
        var customer2 = CustomerFactory.CreateCustomer(vehicles: [vehicle2]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

        await _dbContext.Customers.AddAsync(customer1);
        await _dbContext.Customers.AddAsync(customer2);
        await _dbContext.Vehicles.AddAsync(vehicle1);
        await _dbContext.Vehicles.AddAsync(vehicle2);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.SaveChangesAsync(default);

        var baseTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        // Create 2 work orders for vehicle1
        var workOrder1Vehicle1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle1.Id,
            startAt: baseTime,
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        var workOrder2Vehicle1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle1.Id,
            startAt: baseTime.AddHours(2),
            laborId: employee.Id,
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        // Create 1 work order for vehicle2
        var workOrderVehicle2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle2.Id,
            startAt: baseTime.AddHours(4),
            laborId: employee.Id,
            spot: Spot.C,
            repairTasks: [repairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder1Vehicle1);
        await _dbContext.WorkOrders.AddAsync(workOrder2Vehicle1);
        await _dbContext.WorkOrders.AddAsync(workOrderVehicle2);
        await _dbContext.SaveChangesAsync(default);

        // Query only for vehicle1's work orders
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            VehicleId: vehicle1.Id);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);

        var paginatedList = result.Value;
        Assert.Equal(2, paginatedList.TotalCount); // Only 2 work orders for vehicle1
        Assert.Equal(2, paginatedList.Items.Count);

        // Verify all returned items are for vehicle1
        Assert.All(paginatedList.Items, item =>
        {
            Assert.Equal(vehicle1.Id, item.Vehicle.VehicleId);
            Assert.Equal(customer1.Name, item.Customer);
        });
    }

    [Fact]
    public async Task Handle_WithLaborFilter_ShouldReturnOnlyMatchingLabor()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();

        // Create two different employees
        var employee1 = EmployeeFactory.CreateEmployee(
            firstName: "John",
            lastName: "Smith").Value;
        var employee2 = EmployeeFactory.CreateEmployee(
            firstName: "Jane",
            lastName: "Doe").Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.Employees.AddAsync(employee1);
        await _dbContext.Employees.AddAsync(employee2);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.SaveChangesAsync(default);

        var baseTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        // Create 2 work orders for employee1
        var workOrder1Employee1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime,
            laborId: employee1.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        var workOrder2Employee1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime.AddHours(2),
            laborId: employee1.Id,
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        // Create 1 work order for employee2
        var workOrderEmployee2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseTime.AddHours(4),
            laborId: employee2.Id,
            spot: Spot.C,
            repairTasks: [repairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder1Employee1);
        await _dbContext.WorkOrders.AddAsync(workOrder2Employee1);
        await _dbContext.WorkOrders.AddAsync(workOrderEmployee2);
        await _dbContext.SaveChangesAsync(default);

        // Query only for employee1's work orders
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            LaborId: employee1.Id);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);

        var paginatedList = result.Value;
        Assert.Equal(2, paginatedList.TotalCount); // Only 2 work orders for employee1
        Assert.Equal(2, paginatedList.Items.Count);

        // Verify all returned items are assigned to employee1
        Assert.All(paginatedList.Items, item =>
        {
            Assert.Equal("John Smith", item.Labor);
        });
    }

    [Fact]
    public async Task Handle_WithStartDateFrom_ShouldReturnOnlyWorkOrdersStartingAfter()
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

        var baseDate = DateTimeOffset.UtcNow.Date;

        // Create work orders on different dates
        var workOrderDay1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseDate.AddDays(1).AddHours(10),  // Day 1
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        var workOrderDay3 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseDate.AddDays(3).AddHours(10),  // Day 3
            laborId: employee.Id,
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        var workOrderDay5 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseDate.AddDays(5).AddHours(10),  // Day 5
            laborId: employee.Id,
            spot: Spot.C,
            repairTasks: [repairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrderDay1);
        await _dbContext.WorkOrders.AddAsync(workOrderDay3);
        await _dbContext.WorkOrders.AddAsync(workOrderDay5);
        await _dbContext.SaveChangesAsync(default);

        // Query for work orders starting from Day 3 onwards
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            StartDateFrom: baseDate.AddDays(3));

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);

        var paginatedList = result.Value;
        Assert.Equal(2, paginatedList.TotalCount); // Day 3 and Day 5
        Assert.Equal(2, paginatedList.Items.Count);

        // Verify all returned items start on or after Day 3
        Assert.All(paginatedList.Items, item =>
        {
            Assert.True(item.StartAtUtc >= baseDate.AddDays(3));
        });

        // Verify Day 1 is NOT included
        Assert.DoesNotContain(paginatedList.Items, item => item.WorkOrderId == workOrderDay1.Id);
    }

    [Fact]
    public async Task Handle_WithStartDateTo_ShouldReturnOnlyWorkOrdersStartingBefore()
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

        var baseDate = DateTimeOffset.UtcNow.Date;

        // Create work orders on different dates
        var workOrderDay1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseDate.AddDays(1).AddHours(10),  // Day 1
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        var workOrderDay3 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseDate.AddDays(3).AddHours(10),  // Day 3
            laborId: employee.Id,
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        var workOrderDay5 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: baseDate.AddDays(5).AddHours(10),  // Day 5
            laborId: employee.Id,
            spot: Spot.C,
            repairTasks: [repairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrderDay1);
        await _dbContext.WorkOrders.AddAsync(workOrderDay3);
        await _dbContext.WorkOrders.AddAsync(workOrderDay5);
        await _dbContext.SaveChangesAsync(default);

        // Query for work orders starting up to Day 3
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            StartDateTo: baseDate.AddDays(3).AddHours(23).AddMinutes(59));  // End of Day 3

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);

        var paginatedList = result.Value;
        Assert.Equal(2, paginatedList.TotalCount); // Day 1 and Day 3
        Assert.Equal(2, paginatedList.Items.Count);

        // Verify all returned items start on or before Day 3
        Assert.All(paginatedList.Items, item =>
        {
            Assert.True(item.StartAtUtc <= baseDate.AddDays(3).AddHours(23).AddMinutes(59));
        });

        // Verify Day 5 is NOT included
        Assert.DoesNotContain(paginatedList.Items, item => item.WorkOrderId == workOrderDay5.Id);
    }
}