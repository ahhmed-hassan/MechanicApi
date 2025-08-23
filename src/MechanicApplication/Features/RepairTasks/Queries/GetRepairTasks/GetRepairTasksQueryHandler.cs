

using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicApplication.Features.RepairTasks.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicApplication.Features.RepairTasks.Queries.GetRepairTasks;

public sealed class GetRepairTasksQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRepairTasksQuery, ErrorOr<List<RepairTaskDTO>>>
{
    private readonly IAppDbContext _context = context;
    public async Task<ErrorOr<List<RepairTaskDTO>>> Handle(GetRepairTasksQuery request, CancellationToken cancellationToken)
    {
        var repairTasks = await _context.RepairTasks.Include(rt => rt.Parts).AsNoTracking().ToListAsync();
        return repairTasks.ToDtos(); 
    }
}
