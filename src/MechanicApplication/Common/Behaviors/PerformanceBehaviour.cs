using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MechanicApplication.Common.Behaviors;

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger, 
    IUser user, 
    IIdenttiyService identtiyService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private readonly Stopwatch _stopwatch = new();
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdenttiyService _identtiyService = identtiyService;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _stopwatch.Start();
        var response = await next();
        _stopwatch.Stop();
        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
        if (elapsedMilliseconds > 500)
        {
            var requestName = request.GetType().Name;
            var userId = _user.Id;
            string? userName = string.Empty;
            if (!string.IsNullOrEmpty(userId))
            {
                userName = await _identtiyService.GetUserNameAsync(userId);
            }
            _logger.LogWarning(
                "Long Running Request: {Name} {@UserId} {@UserName} {@Request} executed in {ElapsedMilliseconds} ms",
                requestName, userId, userName, request, elapsedMilliseconds);

        }
        return response;
    }
}
