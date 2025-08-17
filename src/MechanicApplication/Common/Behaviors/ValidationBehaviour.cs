using ErrorOr;
using FluentValidation;
using MediatR;

namespace MechanicApplication.Common.Behaviors;

public class ValidationBehaviour
{
    public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
    {
        private readonly IValidator<TRequest>? _validator = validator;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validator != null)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(error => Error.Validation(error.PropertyName, error.ErrorMessage))
                        .ToList();
                    return (dynamic)errors!;
                }
            }
            return await next();
        }
    }
}
