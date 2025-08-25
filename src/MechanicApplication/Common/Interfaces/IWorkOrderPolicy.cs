

using ErrorOr;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicApplication.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);
    Task<bool> IsLaborOccupied(Guid laborId, Guid excludeWorkOrderOrderId, DateTimeOffset startAt, DateTimeOffset endAt);
    Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt);
    Task<ErrorOr<Success>> IsSpotAvailableAsync(Spot spot, DateTimeOffset startAt
        , DateTimeOffset endAt,Guid? excludeWorkOrderId = null, CancellationToken cancellation= default);
    ErrorOr<Success> IsGreaterThanMinimalAppointmentDuration(DateTimeOffset startAt, DateTimeOffset endAt);
}
