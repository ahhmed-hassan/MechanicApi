using MechanicApi.Application.subcutaneoustests.Common;
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
    : IntegrationTestBase(factory)
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
        var originalEndAt = scheduledAt.AddMinutes((int)originalRepairTask.EstimatedDurationInMins);

        // Create initial work order with original repair task
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            startAt: scheduledAt,
            endAt: originalEndAt,
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

        // Verify timing was updated (45 + 30 = 75 minutes)
        var expectedEndAt = scheduledAt.AddMinutes(75);
        Assert.Equal(expectedEndAt, updatedWorkOrder.EndAtUtc);
    }
}