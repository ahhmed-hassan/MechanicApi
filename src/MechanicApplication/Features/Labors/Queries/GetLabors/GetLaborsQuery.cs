using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Labors.DTOs;
namespace MechanicApplication.Features.Labors.Queries.GetLabors;

public sealed record GetLaborsQuery : ICachedQuery<ErrorOr<List<LaborDTO>>>
{
    public string CacheKey => Constants.Cache.Labors.Single;

    public string[] Tags => [Constants.Cache.Labors.Plural];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
