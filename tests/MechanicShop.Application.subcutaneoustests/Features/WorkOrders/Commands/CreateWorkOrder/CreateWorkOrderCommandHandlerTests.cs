using MechanicApi.Application.subcutaneoustests.Common;
using MechanicApi.Controllers;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Features.WorkOrders.Commands.CreateWorkOrder;
[Collection(WebbAppFactoryColllection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.Mediator;
    private readonly IAppDbContext _dbContext = factory.DbContext;

    [Fact]
    public async Task Handle_GivenValidRequest_ShouldCreateWorkORder()
    {
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

        var scheduledAt = DateTimeOffset.UtcNow.AddDays(3).AddHours(10);

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
}
