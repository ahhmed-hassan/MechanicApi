
using FluentValidation;

namespace MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderState>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(x => x.State)
            .IsInEnum()
            .WithErrorCode("WokrOrder.InvalidState")
            .WithMessage("Invalid work order state.");
    }
}
