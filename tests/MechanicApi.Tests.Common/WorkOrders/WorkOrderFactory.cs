using ErrorOr;
//using MechanicDomain. .Common.Results;
using MechanicDomain.RepairTasks;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepaireTasks;

namespace MechanicShop.Tests.Common.WorkOrders;

public static class WorkOrderFactory
{
    public static ErrorOr<WorkOrder> CreateWorkOrder(
        Guid? id = null,
        Guid? vehicleId = null,
        DateTimeOffset? startAt = null,
        DateTimeOffset? endAt = null,
        Guid? laborId = null,
        Spot spot = Spot.A,
        List<RepairTask>? repairTasks = null)
    {
        return WorkOrder.Create(
            id ?? Guid.NewGuid(),
            vehicleId ?? Guid.NewGuid(),
            startAt ?? DateTimeOffset.UtcNow,
            endAt ?? DateTimeOffset.UtcNow.AddHours(1),
            laborId ?? Guid.NewGuid(),
            spot ,
            repairTasks ?? [RepairTaskFactory.CreateRepairTask().Value]);
    }
}