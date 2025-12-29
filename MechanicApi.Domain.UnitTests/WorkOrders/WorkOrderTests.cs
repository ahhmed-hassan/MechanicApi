using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepaireTasks;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdIsEmpty()
    {
        var wo = WorkOrder.Create(
                    id: Guid.Empty,
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.True(wo.IsError);

        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, wo.FirstError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenVehicleIdIsEmpty()
    {
        var wo = WorkOrder.Create(
                           id: Guid.NewGuid(),
                           vehicleId: Guid.Empty,
                           startAt: DateTimeOffset.UtcNow,
                           endAt: DateTimeOffset.UtcNow.AddHours(1),
                           laborId: Guid.NewGuid(),
                           spot: Spot.A,
                           repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.True(wo.IsError);

        Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, wo.FirstError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenNoRepairTasks()
    {
        var wo = WorkOrder.Create(
                           id: Guid.NewGuid(),
                           vehicleId: Guid.NewGuid(),
                           startAt: DateTimeOffset.UtcNow,
                           endAt: DateTimeOffset.UtcNow.AddHours(1),
                           laborId: Guid.NewGuid(),
                           spot: Spot.A,
                           repairTasks: []);

        Assert.True(wo.IsError);

        Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, wo.FirstError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenLaborIdIsEmpty()
    {
        var wo = WorkOrder.Create(
                              id: Guid.NewGuid(),
                              vehicleId: Guid.NewGuid(),
                              startAt: DateTimeOffset.UtcNow,
                              endAt: DateTimeOffset.UtcNow.AddHours(1),
                              laborId: Guid.Empty,
                              spot: Spot.A,
                              repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.True(wo.IsError);

        Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, wo.FirstError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenTimingInvalid()
    {
        var wo = WorkOrder.Create(
                           id: Guid.NewGuid(),
                           vehicleId: Guid.NewGuid(),
                           startAt: DateTimeOffset.UtcNow.AddHours(1),
                           endAt: DateTimeOffset.UtcNow,
                           laborId: Guid.NewGuid(),
                           spot: Spot.A,
                           repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.True(wo.IsError);

        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, wo.FirstError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenSpotInvalid()
    {
        const Spot invalidSpot = (Spot)999;

        var wo = WorkOrder.Create(
                      id: Guid.NewGuid(),
                      vehicleId: Guid.NewGuid(),
                      startAt: DateTimeOffset.UtcNow,
                      endAt: DateTimeOffset.UtcNow.AddHours(1),
                      laborId: Guid.NewGuid(),
                      spot: invalidSpot,
                      repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.True(wo.IsError);

        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, wo.FirstError.Code);
    }

    [Fact]
    public void AddRepairTask_ShouldReturnError_WhenNotEditable()
    {
        var wo = WorkOrder.Create(
                   id: Guid.NewGuid(),
                   vehicleId: Guid.NewGuid(),
                   startAt: DateTimeOffset.UtcNow,
                   endAt: DateTimeOffset.UtcNow.AddHours(1),
                   laborId: Guid.NewGuid(),
                   spot: Spot.A,
                   repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        wo.UpdateState(WorkOrderState.InProgress);
        wo.UpdateState(WorkOrderState.Completed);

        var result = wo.AddRepairTask(RepairTaskFactory.CreateRepairTask().Value);

        Assert.True(result.IsError);
        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public void UpdateLabor_ShouldReturnError_WhenLaborIdEmpty()
    {
        var wo = WorkOrder.Create(
                       id: Guid.NewGuid(),
                       vehicleId: Guid.NewGuid(),
                       startAt: DateTimeOffset.UtcNow,
                       endAt: DateTimeOffset.UtcNow.AddHours(1),
                       laborId: Guid.NewGuid(),
                       spot: Spot.A,
                       repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateLabor(Guid.Empty);

        Assert.True(result.IsError);
        Assert.Equal(WorkOrderErrors.LaborIdEmpty(wo.Id.ToString()).Code, result.FirstError.Code);
    }

    [Fact]
    public void UpdateSpot_ShouldReturnError_WhenSpotInvalid()
    {
        var wo = WorkOrder.Create(
               id: Guid.NewGuid(),
               vehicleId: Guid.NewGuid(),
               startAt: DateTimeOffset.UtcNow,
               endAt: DateTimeOffset.UtcNow.AddHours(1),
               laborId: Guid.NewGuid(),
               spot: Spot.A,
               repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        const Spot invalidSpot = (Spot)999;
        var result = wo.UpdateSpot(invalidSpot);

        Assert.True(result.IsError);
        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.FirstError.Code);
    }

    [Fact]
    public void UpdateTiming_ShouldReturnError_WhenInvalid()
    {
        var wo = WorkOrder.Create(
                          id: Guid.NewGuid(),
                          vehicleId: Guid.NewGuid(),
                          startAt: DateTimeOffset.UtcNow,
                          endAt: DateTimeOffset.UtcNow.AddHours(1),
                          laborId: Guid.NewGuid(),
                          spot: Spot.A,
                          repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateTiming(DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow);

        Assert.True(result.IsError);
        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.FirstError.Code);
    }

    [Fact]
    public void UpdateState_ShouldReturnError_WhenTransitionInvalid()
    {
        var wo = WorkOrder.Create(
                      id: Guid.NewGuid(),
                      vehicleId: Guid.NewGuid(),
                      startAt: DateTimeOffset.UtcNow,
                      endAt: DateTimeOffset.UtcNow.AddHours(1),
                      laborId: Guid.NewGuid(),
                      spot: Spot.A,
                      repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateState(WorkOrderState.Completed);

        Assert.True(result.IsError);
        Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, WorkOrderState.Completed).Code, result.FirstError.Code);
    }

    [Fact]
    public void UpdateLabor_ShouldReturnSuccess_AndSetNewLaborId()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newLabor = Guid.NewGuid();
        var result = wo.UpdateLabor(newLabor);

        Assert.False(result.IsError);
        Assert.Equal(newLabor, wo.LaborId);
    }

    [Fact]
    public void UpdateSpot_ShouldReturnSuccess_AndSetNewSpot()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateSpot(Spot.B);

        Assert.False(result.IsError);
        Assert.Equal(Spot.B, wo.Spot);
    }

    [Fact]
    public void UpdateTiming_ShouldReturnSuccess_AndSetNewTiming()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newStart = wo.StartAtUtc.AddHours(2);
        var newEnd = newStart.AddHours(1);
        var result = wo.UpdateTiming(newStart, newEnd);

        Assert.False(result.IsError);
        Assert.Equal(newStart, wo.StartAtUtc);
        Assert.Equal(newEnd, wo.EndAtUtc);
    }

    [Fact]
    public void UpdateState_ShouldReturnSuccess_AndSetStateToInProgress()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateState(WorkOrderState.InProgress);

        Assert.False(result.IsError);
        Assert.Equal(WorkOrderState.InProgress, wo.State);
    }
}