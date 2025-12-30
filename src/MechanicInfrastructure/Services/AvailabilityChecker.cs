
using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Extensions;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.WorkOrders.Enums;
using MechanicInfrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MechanicInfrastructure.Services;

public class AvailabilityChecker(
    IAppDbContext context, 
    IOptions<AppSettings> appSettings
    ) : IWorkOrderPolicy
{
    private readonly IAppDbContext _context = context;
    private readonly AppSettings _appSettings = appSettings.Value;
    public async Task<bool> IsLaborOccupied(Guid laborId, Guid? excludeWorkOrderOrderId, DateTimeOffset startAt, DateTimeOffset endAt)
    {
        return await _context.WorkOrders
            .Where(wo =>
                wo.LaborId == laborId &&
                wo.Id != excludeWorkOrderOrderId)
            .AnyRangeOverlappsAsync(startAt, endAt)
            ;
    }

    public async Task<ErrorOr<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default)
    {
        var isOccupied = await _context.WorkOrders
            .Where(wo =>
                wo.Spot == spot &&
                (excludeWorkOrderId == null || wo.Id != excludeWorkOrderId))
            .AnyRangeOverlappsAsync(startAt, endAt, ct);
            

        return isOccupied
            ? Error.Conflict("MechanicShop_Spot_Full", "The selected time slot is unavailable for the requested services.")
            : Result.Success;

    }
    public async Task<bool> IsVehicleAlreadyScheduled(
        Guid vehicleId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid? excludedWorkOrderId = null)
    {
        return await _context.WorkOrders
            .Where(wo =>
                wo.VehicleId == vehicleId &&
                (excludedWorkOrderId == null || wo.Id != excludedWorkOrderId))
             .AnyRangeOverlappsAsync(startAt, endAt)
            ;
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        var dateOfTheDay = startAt.Date;
        var opening = dateOfTheDay.Add(_appSettings.OpeningTime.ToTimeSpan());
        var closing = dateOfTheDay.Add(_appSettings.ClosingTime.ToTimeSpan());
        return startAt < opening || endAt > closing;
    }
    public ErrorOr<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if ((endAt - startAt)
            < TimeSpan.FromMinutes(_appSettings.MinimumAppointmentDurationInMinutes)
            )
            return Error.Validation(
                "WorkOrder_TooShort",
                $"WorkOrder duration must be at least {_appSettings.MinimumAppointmentDurationInMinutes} minutes."
                );
        else
            return new ErrorOr.Success(); 
    }
}
