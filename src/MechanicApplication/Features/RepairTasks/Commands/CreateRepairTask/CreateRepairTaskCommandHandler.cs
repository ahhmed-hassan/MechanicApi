using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicApplication.Features.RepairTasks.Mappers;
using MechanicDomain.RepairTasks;
using MechanicDomain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;


namespace MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandHandler(
    ILogger<CreateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    )
    : IRequestHandler<CreateRepairTaskCommand, ErrorOr<RepairTaskDTO>>
{
    private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<RepairTaskDTO>> Handle(CreateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.RepairTasks
            .AnyAsync(repairTask => EF.Functions.Like(repairTask.Name, request.Name)
            , cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning("Duplicate part time '{PartName}'", request.Name);
            return RepairTaskErrors.DuplicateName;
        }
        List<ErrorOr<Part>> parts = request.Parts.Select(p =>
            Part.Create(Guid.NewGuid(), p.Name, p.Cost, p.Quantity)).ToList();

        if (parts.Where(p => p.IsError).SelectMany(e => e.Errors).ToList() is { Count: > 0 } errors)
            return errors;

        var createRepairTaskResult = RepairTask.Create(
            id: Guid.NewGuid(),
            name: request.Name!,
            estimatedDurationInMins: request.EstimatedDurationInMins!.Value,
            laborCost: request.LaborCost,
            parts: parts.ConvertAll(p => p.Value));

        return await createRepairTaskResult.ThenDoAsync(async repairTask =>
        {
            _context.RepairTasks.Add(repairTask);
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveByTagAsync(Constants.Cache.RepairTasks.Single, cancellationToken);
        })
            .Then(repairTask => repairTask.ToDto());


    }
}
