

using MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    private readonly UpdateWorkOrderRepairTasksCommandValidator _validator;

    public UpdateWorkOrderRepairTasksCommandValidatorTests()
    {
        _validator = new UpdateWorkOrderRepairTasksCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_WorkOrderId_Is_Empty()
    {
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.Empty,
            RepairTasksIds: [Guid.NewGuid()]);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.WorkOrderId));
        Assert.Contains(result.Errors, e => e.ErrorCode == "WorkOrderId_Required");
    }

    [Fact]
    public void Should_Have_Error_When_RepairTaskIds_Is_Empty()
    {
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTasksIds: []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.RepairTasksIds));
        Assert.Contains(result.Errors, e => e.ErrorCode == "RepairTasks_Required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTasksIds: [Guid.NewGuid(), Guid.NewGuid()]);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
