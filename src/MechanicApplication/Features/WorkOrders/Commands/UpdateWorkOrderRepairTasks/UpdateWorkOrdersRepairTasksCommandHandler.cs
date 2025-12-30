

using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrdersRepairTasksCommandHandler
    (
    IAppDbContext context,
    ILogger<UpdateWorkOrdersRepairTasksCommandHandler> logger
    )
    : IRequestHandler<UpdateWorkOrderRepairTasksCommand, ErrorOr<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdateWorkOrdersRepairTasksCommandHandler> _logger = logger;
    public async Task<ErrorOr<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(workOrder => workOrder.RepairTasks)
            .FirstOrDefaultAsync(workOrder => workOrder.Id == request.WorkOrderId);

        var requestedRepairTasks = await _context.RepairTasks
            .Where(repairTask => request.RepairTasksIds.Contains(repairTask.Id))
            .ToListAsync(cancellationToken);

        if(workOrder is null)
        {
            logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        var result = await requestedRepairTasks
        .Aggregate(
         workOrder.ClearRepairTasks(),
         (currentResult, requestedTask) => currentResult.Then(_ => workOrder.AddRepairTask(requestedTask))
         )
        .ThenDoAsync(async _ => await _context.SaveChangesAsync(cancellationToken))
         ;

        return result;
    }
}
