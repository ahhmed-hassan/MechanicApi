
using MediatR;

namespace MechanicApplication.Common.Interfaces;

public interface ICachedQuery
{
    string CacheKey { get; }
    string[] Tage { get; }
    TimeSpan Expiration { get; }
}
public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery; 
