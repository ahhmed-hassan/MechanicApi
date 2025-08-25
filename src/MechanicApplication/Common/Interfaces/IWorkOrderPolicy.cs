

using ErrorOr;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicApplication.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);
    ErrorOr<Success> IsGreaterThanMinimalAppointmentDuration(DateTimeOffset startAt, DateTimeOffset endAt);
}
