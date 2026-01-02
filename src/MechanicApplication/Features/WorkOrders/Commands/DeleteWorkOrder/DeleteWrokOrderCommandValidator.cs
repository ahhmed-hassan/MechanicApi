using FluentValidation;

namespace MechanicApplication.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWrokOrderCommandValidator : AbstractValidator<DeleteWorkOrderCommand>
{
    public DeleteWrokOrderCommandValidator()
    {
        RuleFor(x=> x.wokrOrderId)
            .NotEmpty()
            .WithMessage("Work Order Id must be provided.")
            .WithErrorCode("WorkOrderId_Required")
            ;
    }
}
