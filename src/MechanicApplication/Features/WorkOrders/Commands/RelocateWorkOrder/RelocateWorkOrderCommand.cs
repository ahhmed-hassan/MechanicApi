using ErrorOr;
using FluentValidation;
using MechanicDomain.WorkOrders.Enums;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed record RelocateWorkOrderCommand(
    Guid Id, 
    DateTimeOffset NewStartAt, 
    Spot NewSpot
    ) : IRequest<ErrorOr<Updated>>;

public sealed class RelocateWorkorderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
    public RelocateWorkorderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.NewStartAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("New start time must be in the future");

        RuleFor(x => x.NewSpot)
            .IsInEnum(); 
    }
}
