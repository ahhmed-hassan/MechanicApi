
using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.RepairTasks.Commands.RemoveRepairTask;

public sealed class RemoveRepairTaskCommandHandler(
    ILogger<RemoveRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    ) : IRequestHandler<RemoveRepairTaskCommand, ErrorOr<Deleted>>
{
    private readonly ILogger<RemoveRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<Deleted>> Handle(RemoveRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks.FindAsync([request.RepairTaskId]);
        if(repairTask is null)
        {
            _logger.LogWarning("Repair Task with ID {RepairTaskId} not found.", request.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }
        //No Include here - we just need to check if any work orders reference this repair task, not actually loading the RepairTasks to use 
        var isInUse = await _context.WorkOrders.AsNoTracking()
            .SelectMany(wo => wo.RepairTasks)
            .AnyAsync(rt => rt.Id == request.RepairTaskId, cancellationToken);

        if(isInUse)
        {
            _logger.LogWarning("Repair Task with ID {RepairTaskId} is associated with existing work orders and cannot be deleted.", request.RepairTaskId);
            return RepairTaskErrors.InUse;
        }
        _context.RepairTasks.Remove(repairTask);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Repair Task with ID {RepairTaskId} has been removed successfully.", request.RepairTaskId);
        return Result.Deleted;


        
    }
}
