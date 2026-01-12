using MechanicApi.Application.subcutaneoustests.Common;
using MechanicApplication.Common.Errors;
using MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicDomain.RepairTasks.Enums;
using MechanicDomain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

[Collection(WebbAppFactoryColllection.CollectionName)]
public class UpdateWorkOrderRepairTasksCommandHandlerTests(WebAppFactory factory)
    : IntegrationTestsBase(factory)
{
    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateRepairTasks()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        // Original repair task
        var originalRepairTask = RepairTaskFactory.CreateRepairTask(
            name: "Oil Change",
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

        // New repair tasks to update to
        var newRepairTask1 = RepairTaskFactory.CreateRepairTask(
            name: "Brake Inspection",
            repairDurationInMinutes: RepairDurationInMinutes.Min45).Value;
        var newRepairTask2 = RepairTaskFactory.CreateRepairTask(
            name: "Tire Rotation",
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.RepairTasks.AddAsync(originalRepairTask);
        await _dbContext.RepairTasks.AddAsync(newRepairTask1);
        await _dbContext.RepairTasks.AddAsync(newRepairTask2);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        // Create initial work order with original repair task
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: scheduledAt,
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [originalRepairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder);
        await _dbContext.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTasksIds: [newRepairTask1.Id, newRepairTask2.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.False(result.IsError);

        // Verify work order was updated in database
        var updatedWorkOrder = await _factory.DbContext.WorkOrders
            .Include(w => w.RepairTasks)
            .FirstOrDefaultAsync(w => w.Id == workOrder.Id);

        Assert.NotNull(updatedWorkOrder);
        Assert.Equal(2, updatedWorkOrder.RepairTasks.Count());
        Assert.Contains(updatedWorkOrder.RepairTasks, rt => rt.Id == newRepairTask1.Id);
        Assert.Contains(updatedWorkOrder.RepairTasks, rt => rt.Id == newRepairTask2.Id);

    }

    [Fact]
    public async Task Handle_WithNonExistentWorkOrder_ShouldFail()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        // Create repair tasks that exist, but NO work order
        var repairTask1 = RepairTaskFactory.CreateRepairTask(
            name: "Brake Inspection",
            repairDurationInMinutes: RepairDurationInMinutes.Min45).Value;
        var repairTask2 = RepairTaskFactory.CreateRepairTask(
            name: "Tire Rotation",
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

        await _dbContext.RepairTasks.AddAsync(repairTask1);
        await _dbContext.RepairTasks.AddAsync(repairTask2);
        await _dbContext.SaveChangesAsync(default);

        // Try to update a work order that doesn't exist
        var nonExistentWorkOrderId = Guid.NewGuid();
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: nonExistentWorkOrderId,
            RepairTasksIds: [repairTask1.Id, repairTask2.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithPartiallyExistingRepairTasks_ShouldFail()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        // Original repair task for the work order
        var originalRepairTask = RepairTaskFactory.CreateRepairTask(
            name: "Oil Change",
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

        // Only ONE new repair task exists
        var existingRepairTask = RepairTaskFactory.CreateRepairTask(
            name: "Brake Inspection",
            repairDurationInMinutes: RepairDurationInMinutes.Min45).Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.RepairTasks.AddAsync(originalRepairTask);
        await _dbContext.RepairTasks.AddAsync(existingRepairTask);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: scheduledAt,
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [originalRepairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder);
        await _dbContext.SaveChangesAsync(default);

        // Try to update with one existing and one non-existent repair task
        var nonExistentRepairTaskId = Guid.NewGuid();
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTasksIds: [existingRepairTask.Id, nonExistentRepairTaskId]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithUpdatedTasksExceedingOperatingHours_ShouldFail()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        // Original task: 30 minutes, ending at 17:30
        var originalRepairTask = RepairTaskFactory.CreateRepairTask(
            name: "Oil Change",
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

        // New task: Very long duration that will exceed operating hours (18:00)
        // If we start at 10:00 and add 9 hours (540 min), we'd end at 19:00 - outside hours!
        var longRepairTask = RepairTaskFactory.CreateRepairTask(
            name: "Engine Overhaul",
            repairDurationInMinutes: RepairDurationInMinutes.Min180).Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.RepairTasks.AddAsync(originalRepairTask);
        await _dbContext.RepairTasks.AddAsync(longRepairTask);
        await _dbContext.SaveChangesAsync(default);

        // Schedule exactly one hour before closing time, so we have 30 minutes left before closing after making this task. 
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1)
            .Add(_factory.AppSettings.ClosingTime.ToTimeSpan())
            .AddHours(-1)
            ;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: scheduledAt,
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [originalRepairTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder);
        await _dbContext.SaveChangesAsync(default);

        // Try to update with a long task that would end at 19:00 (past closing time of 18:00)
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTasksIds: [longRepairTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        //Care only about the code itself 
        Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHour(DateTimeOffset.Now, DateTimeOffset.Now).Code, result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithSpotUnavailable_ShouldFail()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        // Short task for initial setup
        var shortTask = RepairTaskFactory.CreateRepairTask(
            name: "Quick Inspection",
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value; // 1 hour

        // Long task that will cause overlap
        var longTask = RepairTaskFactory.CreateRepairTask(
            name: "Extended Repair",
            repairDurationInMinutes: RepairDurationInMinutes.Min180).Value; // 3 hours

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Employees.AddAsync(employee1);
        await _dbContext.Employees.AddAsync(employee2);
        await _dbContext.RepairTasks.AddAsync(shortTask);
        await _dbContext.RepairTasks.AddAsync(longTask);
        await _dbContext.SaveChangesAsync(default);

        var baseTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        // WorkOrder1: Spot A, 12:00 - 14:00 (won't be modified)
        var workOrder1StartAt = baseTime.AddHours(2); // 12:00
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle1.Id,
            startAt: workOrder1StartAt,
            laborId: employee1.Id,
            spot: Spot.A,
            repairTasks: [shortTask]).Value;

        // WorkOrder2: Spot A, 10:00 - 11:00 (currently no overlap with WorkOrder1)
        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle2.Id,
            startAt: baseTime, // 10:00
            laborId: employee2.Id,
            spot: Spot.A, // ✅ Same spot as WorkOrder1
            repairTasks: [shortTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder1);
        await _dbContext.WorkOrders.AddAsync(workOrder2);
        await _dbContext.SaveChangesAsync(default);

        // Try to update WorkOrder2 with a long task
        // This would extend it from 10:00 - 13:00, which overlaps with WorkOrder1 (12:00 - 14:00)
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder2.Id,
            RepairTasksIds: [longTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("MechanicShop_Spot_Full", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithLaborOccupied_ShouldFail()
    {
        // Arrange
        Assert.True(IsEmptyDatabase());

        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value; // ✅ Same employee for both

        var shortTask = RepairTaskFactory.CreateRepairTask(
            name: "Quick Inspection",
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value; // 1 hour

        var longTask = RepairTaskFactory.CreateRepairTask(
            name: "Extended Repair",
            repairDurationInMinutes: RepairDurationInMinutes.Min180).Value; // 3 hours

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle1);
        await _dbContext.Vehicles.AddAsync(vehicle2);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.RepairTasks.AddAsync(shortTask);
        await _dbContext.RepairTasks.AddAsync(longTask);
        await _dbContext.SaveChangesAsync(default);

        var baseTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        // WorkOrder1: Same employee, 12:00 - 14:00, Spot A (won't be modified)
        var workOrder1StartAt = baseTime.AddHours(2); // 12:00
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle1.Id,
            startAt: workOrder1StartAt,
            laborId: employee.Id, // ✅ Same employee
            spot: Spot.A,
            repairTasks: [shortTask]).Value;

        // WorkOrder2: Same employee, 10:00 - 11:00, Spot B (currently no conflict)
        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle2.Id,
            startAt: baseTime, // 10:00
            laborId: employee.Id, // ✅ Same employee
            spot: Spot.B, // Different spot, so no spot conflict
            repairTasks: [shortTask]).Value;

        await _dbContext.WorkOrders.AddAsync(workOrder1);
        await _dbContext.WorkOrders.AddAsync(workOrder2);
        await _dbContext.SaveChangesAsync(default);

        // Try to update WorkOrder2 with a long task
        // This would extend it from 10:00 - 13:00, which overlaps with WorkOrder1 (12:00 - 14:00)
        // Same employee cannot work on both overlapping work orders
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder2.Id,
            RepairTasksIds: [longTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.FirstError.Code);
    }
}