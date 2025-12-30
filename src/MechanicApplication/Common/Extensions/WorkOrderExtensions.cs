using MechanicDomain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MechanicApplication.Common.Extensions;

public static class WorkOrderExtensions
{
    public static IQueryable<WorkOrder> WhereRangeOverlapps(
        this IQueryable<WorkOrder> workOrders,
        DateTimeOffset start,
        DateTimeOffset end
        ) => workOrders
        .Where(ScheduleOverlaps(start, end))
            ;

    public static Task<bool> AnyRangeOverlappsAsync (
        this IQueryable<WorkOrder> workOrders,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default
        ) =>  workOrders
        .WhereRangeOverlapps(start, end)
        .AnyAsync(cancellationToken)
            ;

    public static bool RangeOverlapps(
        this  WorkOrder workOrder,
        DateTimeOffset start,
        DateTimeOffset end
        ) => workOrder.StartAtUtc < end && workOrder.EndAtUtc > start
            ;

    public static Expression<Func<WorkOrder, bool>> ScheduleOverlaps(DateTimeOffset start, DateTimeOffset end)
    {
        return wo => wo.StartAtUtc < end && wo.EndAtUtc > start;
    }
}
