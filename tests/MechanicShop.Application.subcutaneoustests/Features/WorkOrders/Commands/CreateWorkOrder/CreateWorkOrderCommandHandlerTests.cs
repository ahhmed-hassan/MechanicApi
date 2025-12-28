using MechanicApi.Application.subcutaneoustests.Common;
using MechanicApi.Controllers;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicDomain.RepairTasks.Enums;
using MechanicDomain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Features.WorkOrders.Commands.CreateWorkOrder;
[Collection(WebbAppFactoryColllection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests(WebAppFactory factory)
    :IntegrationTestBase(factory)
{
    //private readonly IMediator _mediator = factory.Mediator;
    //private readonly IAppDbContext _dbContext = factory.DbContext;

    [Fact]
    public async Task Handle_GivenValidRequest_ShouldCreateWorkORder()
    {
        Assert.True(IsEmptyDatabase());
        //Arrange 
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(10);

        var command = new MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder.CreateWorkOrderCommand(
            Spot : Spot.B, 
            VehicleId : vehicle.Id, 
            StartAt : scheduledAt,
            RepairTasksIds : new List<Guid> { repairTask.Id },
            LaborId : employee.Id
            );

        //Act
        var result = await _mediator.Send(command);

        //Assert
        Assert.False(result.IsError);
        Assert.Equal(Spot.B, result.Value.Spot);
        Assert.Equal(vehicle.Id, result.Value.Vehicle?.VehicleId);
        Assert.Single(result.Value.RepairTasks);
        Assert.Equal(repairTask.Id, result.Value.RepairTasks.First().RepairTaskId);
        Assert.Equal(employee.Id, result.Value.Labor?.LaborId);

    }


    [Fact]
    public async Task Handle_WithMissingRepairTask_ShouldFail()
    {
        Assert.True(IsEmptyDatabase());
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;
        
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(default);

        var fakeRepairTaskId = Guid.NewGuid();

        var scheduledAt = DateTimeOffset.UtcNow.Date
        .AddDays(1)
        .AddHours(11);

        var command = new CreateWorkOrderCommand(
            Spot: Spot.C,
            VehicleId: vehicle.Id,
            StartAt: scheduledAt,
            RepairTasksIds: [fakeRepairTaskId],
            LaborId: employee.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WithOutsideOperatingHours_ShouldFail()
    {
        Assert.True(IsEmptyDatabase());
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;


        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
             .AddDays(1)
             .AddHours(4);

        var command = new CreateWorkOrderCommand(Spot.B, vehicle.Id, scheduledAt, [repairTask.Id], employee.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        //We are interested in the error code itself here not the specific errr message resulted from it. 
        Assert.Contains(result.Errors,
            e => e.Code == ApplicationErrors.WorkOrderOutsideOperatingHour(DateTimeOffset.MinValue, DateTimeOffset.MinValue).Code);
    }

    [Fact]
    public async Task Handle_WithShortDuration_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min15).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
         .AddDays(1)
         .AddHours(12);

        var command = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [repairTask.Id], employee.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder_TooShort");
    }

    [Fact]
    public async Task Handle_WithMissingVehicle_ShouldFail()
    {
        Assert.True(IsEmptyDatabase());
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
              .AddDays(1)
              .AddHours(13);

        var command = new CreateWorkOrderCommand(Spot.C, Guid.NewGuid(), scheduledAt, [repairTask.Id], employee.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WithMissingLabor_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
             .AddDays(1)
             .AddHours(14);

        var command = new CreateWorkOrderCommand(Spot.C, vehicle.Id, scheduledAt, [repairTask.Id], Guid.NewGuid());
        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WithVehicleConflict_ShouldFail()
    {
        Assert.True(IsEmptyDatabase());
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee1);
        await _dbContext.Employees.AddAsync(employee2);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
           .AddDays(1)
           .AddHours(15);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [repairTask.Id], employee1.Id);
        var command2 = new CreateWorkOrderCommand(Spot.B, vehicle.Id, scheduledAt, [repairTask.Id], employee2.Id);

        await _mediator.Send(command1);
        var result = await _mediator.Send(command2);

        Assert.True(result.IsError);
        Assert.Equal("Vehicle_Overlapping_WorkOrders", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithLaborConflict_ShouldFail()
    {
        Assert.True(IsEmptyDatabase());
        var customer1 = CustomerFactory.CreateCustomer().Value;
        var vehicle1 = customer1.Vehicles.First();
        var customer2 = CustomerFactory.CreateCustomer().Value;
        var vehicle2 = customer2.Vehicles.First();

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _dbContext.Customers.AddAsync(customer1);
        await _dbContext.Customers.AddAsync(customer2);
        await _dbContext.Vehicles.AddAsync(vehicle1);
        await _dbContext.Vehicles.AddAsync(vehicle2);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
                  .AddDays(1)
                  .AddHours(16);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle1.Id, scheduledAt, [repairTask.Id], employee.Id);
        var command2 = new CreateWorkOrderCommand(Spot.B, vehicle2.Id, scheduledAt, [repairTask.Id], employee.Id);

        await _mediator.Send(command1);
        var result = await _mediator.Send(command2);

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithUnavailableSpot_ShouldFail()
    {
        Assert.True(IsEmptyDatabase());
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.Vehicles.AddAsync(vehicle1);
        await _dbContext.Vehicles.AddAsync(vehicle2);
        await _dbContext.RepairTasks.AddAsync(repairTask);
        await _dbContext.Employees.AddAsync(employee1);
        await _dbContext.Employees.AddAsync(employee2);
        await _dbContext.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(17);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle1.Id, scheduledAt, [repairTask.Id], employee1.Id);
        var command2 = new CreateWorkOrderCommand(Spot.A, vehicle2.Id, scheduledAt, [repairTask.Id], employee2.Id);

        await _mediator.Send(command1);
        var result = await _mediator.Send(command2);

        Assert.True(result.IsError);
        Assert.Equal("MechanicShop_Spot_Full", result.FirstError.Code);
    }
}
