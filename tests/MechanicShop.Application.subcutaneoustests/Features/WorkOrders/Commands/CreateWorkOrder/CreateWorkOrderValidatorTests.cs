using MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicDomain.WorkOrders.Enums;
using System.ComponentModel.DataAnnotations;
using Xunit;
namespace MechanicApi.Application.subcutaneoustests.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderValidatorTests
{
    private readonly CreateWorkOrderCommandValdiator _validator = new CreateWorkOrderCommandValdiator();

    [Fact]
    public void Validate_ShouldReturnError_WhenVehicleIdIsEmpty()
    {
        // Arrange
        var command = new CreateWorkOrderCommand(
          Spot: Spot.A,
          VehicleId: Guid.Empty,
          StartAt: DateTime.UtcNow.AddHours(1),
          RepairTasksIds: [Guid.NewGuid()],
          LaborId: Guid.NewGuid()
          );

        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.VehicleId) &&
                                           e.ErrorMessage.Contains("VehicleId must not be empty"));
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenStartAtIsInThePast()
    {
        // Arrange
        var command = new CreateWorkOrderCommand(
          Spot: Spot.A,
          VehicleId: Guid.NewGuid(),
          StartAt: DateTime.UtcNow.AddHours(-1),
          RepairTasksIds: [Guid.NewGuid()],
          LaborId: Guid.NewGuid()
          );
        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.StartAt)
                                           &&  e.ErrorMessage.Contains("StartAt must be in the future")
                                           );
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenRepairTasksIdsIsEmpty()
    {
        // Arrange
        var command = new CreateWorkOrderCommand(
          Spot: Spot.A,
          VehicleId: Guid.NewGuid(),
          StartAt: DateTime.UtcNow.AddHours(1),
          RepairTasksIds: [],
          LaborId: Guid.NewGuid()
          );
        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.RepairTasksIds)
                                           && e.ErrorMessage.Contains("RepairTasksIds must contain at least one item")
                                           );
    }
}
