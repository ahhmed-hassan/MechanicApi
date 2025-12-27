
using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.WorkOrders.Enums;
using Microsoft.EntityFrameworkCore;

namespace MechanicInfrastructure.Services;

public class AvailabilityChecker(IAppDbContext context) : IWorkOrderPolicy
{
    private readonly IAppDbContext _context = context;
    public async Task<bool> IsLaborOccupied(Guid laborId, Guid? excludeWorkOrderOrderId, DateTimeOffset startAt, DateTimeOffset endAt)
    {
        return await _context.WorkOrders
            .AnyAsync(wo =>
                wo.LaborId == laborId &&
                wo.Id != excludeWorkOrderOrderId &&
                startAt < wo.EndAtUtc &&
                endAt > wo.StartAtUtc
            );
    }

    public async Task<ErrorOr<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default)
    {
        var isOccupied = await _context.WorkOrders
            .AnyAsync(wo =>
                wo.Spot == spot &&
                (excludeWorkOrderId == null || wo.Id != excludeWorkOrderId) &&
                startAt < wo.EndAtUtc &&
                endAt > wo.StartAtUtc
            , ct);

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
            .AnyAsync(wo =>
                wo.VehicleId == vehicleId &&
                (excludedWorkOrderId == null || wo.Id != excludedWorkOrderId) &&
                startAt < wo.EndAtUtc &&
                endAt > wo.StartAtUtc
            );
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration)
    {
        throw new NotImplementedException();
    }

    public ErrorOr<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        throw new NotImplementedException();
    }
}
