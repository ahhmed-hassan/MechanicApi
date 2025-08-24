using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandHandler
    (
    ILogger<UpdateRepairTaskCommandHandler> logger,
    IAppDbContext appDbContext
    )
    : IRequestHandler<UpdateRepairTaskCommand, ErrorOr<Updated>>
{
    private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = appDbContext;
    public async Task<ErrorOr<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks.Include(rt => rt.Parts)
            .FirstOrDefaultAsync(rt => rt.Id == request.RepairTaskId, cancellationToken);
        if (repairTask is null)
        {
            _logger.LogWarning("Repair task with ID {RepairTaskId} not found.", request.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }
        var validatedParts = new List<Part>();

        foreach (var p in request.CommandParts)
        {
            var partId = p.PartId ?? Guid.NewGuid();

            var partResult = Part.Create(partId, p.Name, p.Cost, p.Quantity);

            if (partResult.IsError)
            {
                return partResult.Errors;
            }

            validatedParts.Add(partResult.Value);
        }
        var updatedRepairTaskResult = repairTask.Update(request.Name,
                                                        request.LaborCost,
                                                        request.EstimatedDurationInMins
                                                        );
        return await updatedRepairTaskResult
            .Then(_ => repairTask.UpsertParts(validatedParts))
            .ThenDoAsync(async _ => await _context.SaveChangesAsync(cancellationToken));

    }
}
