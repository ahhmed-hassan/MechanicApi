using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.WorkOrders;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.WorkOrders.Commands.AssignLabor;

public sealed class AssignLaborCommandHandler(
    ILogger<AssignLaborCommandHandler> logger,
    IAppDbContext context,
    IWorkOrderPolicy availabilityChecker, 
    HybridCache cache

    ) : IRequestHandler<AssignLaborCommand, ErrorOr<Updated>>
{
    private readonly ILogger<AssignLaborCommandHandler> _logger = logger; 
    private readonly IAppDbContext _appDbContext = context;
    private readonly IWorkOrderPolicy _availabilityChecker = availabilityChecker;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<Updated>> Handle(AssignLaborCommand request, CancellationToken cancellationToken)
    {
        var targetWorkOrder = _appDbContext.WorkOrders.FirstOrDefault(wo => wo.Id == request.WokrOrder);
        if (targetWorkOrder == null)
        {
            _logger.LogError("Workorder with Id '{WorkOrderId}' does not exist", request.WokrOrder);
            return ApplicationErrors.WorkOrderNotFound;
        }
        /**FindAsync vs firstOrDefault: 
         * https://dev.to/hamza_darouzi_dotnet/optimal-entity-retrieval-in-net-8-findasync-vs-firstordefaultasync-p4o
         */
        var newLabor = await _appDbContext.Employees.FindAsync(request.Labor, cancellationToken);

        if (newLabor == null)
        {
            _logger.LogError("Labor does not exist {LaborId}", request.Labor);
            return ApplicationErrors.LaborNotFound;
        }
        if (await _availabilityChecker.IsLaborOccupied(request.Labor,
                                                       request.WokrOrder,
                                                       targetWorkOrder.StartAtUtc,
                                                       targetWorkOrder.EndAtUtc))
        {
            _logger.LogError("At the requested time the labor {LaborId} is already busy", request.Labor);
            return ApplicationErrors.LaborOccupied;
        }

        var updateLaborResult = targetWorkOrder.UpdateLabor(request.Labor);
        return await updateLaborResult
            .ThenDoAsync(async updateLaborResult =>
            {
                await _appDbContext.SaveChangesAsync(cancellationToken);
                await _cache.RemoveByTagAsync(Constants.Cache.WorkOrders.Single, cancellationToken);
            })
            .Else(errors =>
            {
                foreach (var e in errors)
                    _logger.LogError("[LaborUpdate] {ErorCode}: {ErroDescription}", e.Code, e.Description);
                return errors;
            }
            );
    }
}
