using ErrorOr;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Common.Interfaces;

public class LoggingBehaviour<TRequest>(
    ILogger<TRequest> logger,
    IUser user,
    IIdenttiyService identtiyService
) : IRequestPreProcessor<TRequest>
    where TRequest : IRequest

{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdenttiyService _identtiyService = identtiyService;
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = request.GetType().Name;
        var userId = _user.Id;
        string? userName = string.Empty;
        if(!string.IsNullOrEmpty(userId))
        {
            userName = await _identtiyService.GetUserNameAsync(userId);
        }
        _logger.LogInformation(
            "Request: {Name} {@UserId} {@UserName} {@Request}", requestName, userId, userName, request);
    }
}
