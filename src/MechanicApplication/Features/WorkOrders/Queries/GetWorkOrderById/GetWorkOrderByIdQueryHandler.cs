using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.WorkOrders.Dtos;
using MechanicApplication.Features.WorkOrders.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryHandler(
    IAppDbContext context,
    ILogger<GetWorkOrderByIdQueryValidator> logger

    )
    : IRequestHandler<GetWorkOrderByIdQuery, ErrorOr<WorkOrderDTO>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetWorkOrderByIdQueryValidator> _logger = logger;
    public async Task<ErrorOr<WorkOrderDTO>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(a=> a.RepairTasks)
                .ThenInclude(r=> r.Parts)
            .Include(a => a.Labor)
            .Include(a=> a.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(a=> a.Invoice)

            .FirstOrDefaultAsync(wo => wo.Id == request.id, cancellationToken);

        if(workOrder is null)
        {
            _logger.LogWarning("Work order with ID {WorkOrderId} not found.", request.id);
            return ApplicationErrors.WorkOrderNotFound;
        }

        return WorkOrderMapper.ToDto(workOrder);
    }
}
