

using ErrorOr;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicApplication.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutsideOperatingHours(DateTimeOffset startAt, DateTimeOffset endAt);

    Task<bool> IsLaborOccupied(Guid laborId, Guid? excludedWorkOrderId, DateTimeOffset startAt, DateTimeOffset endAt);

    Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null);

    Task<ErrorOr<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default);

    ErrorOr<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);
}
