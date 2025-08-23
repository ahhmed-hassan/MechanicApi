

using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.RepairTasks.DTOs;

namespace MechanicApplication.Features.RepairTasks.Queries.GetRepairTasks;

public sealed record GetRepairTasksQuery : ICachedQuery<ErrorOr<List<RepairTaskDTO>>>
{
    public string CacheKey => Constants.Cache.RepairTasks.Single;

    public string[] Tags => [Constants.Cache.RepairTasks.Plural];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
