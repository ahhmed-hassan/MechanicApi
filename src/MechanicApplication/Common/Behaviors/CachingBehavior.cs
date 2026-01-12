
using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    ILogger<CachingBehavior<TRequest, TResponse>> logger,
    HybridCache cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Pass all non-cache requests through
        if (request is not ICachedQuery cachedQuery)
        {
            return await next();
        }
        _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);
        var cacheHit = false;
        var result = await _cache.GetOrCreateAsync<TResponse>(
            key: cachedQuery.CacheKey,
            factory: async _ =>
            {
                cacheHit = false;  // Mark as cache miss
                return default!;   // Return default, we'll populate below
            },
            options: new HybridCacheEntryOptions { Flags = HybridCacheEntryFlags.DisableUnderlyingData },
            cancellationToken: cancellationToken
        );

        if (!cacheHit || EqualityComparer<TResponse>.Default.Equals(result, default))
        {
            result = await next(cancellationToken);
            if (result is IErrorOr {IsError : false})
            {
                _logger.LogInformation("Caching result for {RequestName} with key {CacheKey}", typeof(TRequest).Name, cachedQuery.CacheKey);
                await _cache.SetAsync(
                    cachedQuery.CacheKey,
                    result,
                    new HybridCacheEntryOptions { Expiration = cachedQuery.Expiration },
                    cachedQuery.Tags,
                    cancellationToken);
            }
        }

        return result;
    }
}

