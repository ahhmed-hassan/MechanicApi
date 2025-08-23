

using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicApplication.Features.RepairTasks.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.RepairTasks.Queries.GetRepiarTaskById;

public class GetRepiarTaskByIdQueryHandler(
    ILogger<GetRepiarTaskByIdQueryHandler> logger,
    IAppDbContext context
    ) : IRequestHandler<GetRepairTaskByIdQuery, ErrorOr<RepairTaskDTO>>
{
    private readonly ILogger _logger = logger; 
    private readonly IAppDbContext _context = context;
    public async Task<ErrorOr<RepairTaskDTO>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks.AsNoTracking().Include(rp => rp.Parts)
            .FirstOrDefaultAsync(rp => rp.Id == request.RepairTaskId, cancellationToken);  
        if(repairTask is null)
        {
            _logger.LogWarning("Repair task with ID {RepairTaskId} not found.", request.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }
        return repairTask.ToDto();
    }
}
