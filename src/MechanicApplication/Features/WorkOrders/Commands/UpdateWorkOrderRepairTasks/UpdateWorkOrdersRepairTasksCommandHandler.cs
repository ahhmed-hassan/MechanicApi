

using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrdersRepairTasksCommandHandler
    (
    IAppDbContext context,
    ILogger<UpdateWorkOrdersRepairTasksCommandHandler> logger, 
    HybridCache cache, 
    IWorkOrderPolicy availablilityChecker 
    )
    : IRequestHandler<UpdateWorkOrderRepairTasksCommand, ErrorOr<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdateWorkOrdersRepairTasksCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _availablilityChecker = availablilityChecker;
    public async Task<ErrorOr<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(workOrder => workOrder.RepairTasks)
            .FirstOrDefaultAsync(workOrder => workOrder.Id == request.WorkOrderId);

        var requestedRepairTasks = await _context.RepairTasks
            .Where(repairTask => request.RepairTasksIds.Contains(repairTask.Id))
            .ToListAsync(cancellationToken);

        if (request.RepairTasksIds
            .Except(requestedRepairTasks.Select(x => x.Id))
            .ToList()
            is { Count: > 0 } missingIds)
        {
            _logger.LogError("one or more RepairTasks not found. Missing Ids: {MissingIds}", string.Join(", ", missingIds));
            return ApplicationErrors.RepairTaskNotFound;
        }

        if (workOrder is null)
        {
            logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        var result = await requestedRepairTasks
        .Aggregate(
         workOrder.ClearRepairTasks(),
         (currentResult, requestedTask) => currentResult.Then(_ => workOrder.AddRepairTask(requestedTask))
         )
        .FailIf(_ =>
            _availablilityChecker.IsOutsideOperatingHours(
                workOrder.StartAtUtc, workOrder.EndAtUtc)
            , ApplicationErrors.WorkOrderOutsideOperatingHour(workOrder.StartAtUtc, workOrder.EndAtUtc))
        .ThenAsync(_ =>
              _availablilityChecker.CheckSpotAvailabilityAsync(
            workOrder.Spot, workOrder.StartAtUtc, workOrder.EndAtUtc, workOrder.Id)
              .Then( _ => Result.Updated)
              )
        .ThenDoAsync(async _ =>
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveByTagAsync(Constants.Cache.WorkOrders.Single);

        })
         ;

        return result;
    }
}
