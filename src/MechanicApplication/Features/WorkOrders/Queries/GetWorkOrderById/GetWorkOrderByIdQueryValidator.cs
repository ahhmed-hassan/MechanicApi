using FluentValidation;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
    public GetWorkOrderByIdQueryValidator()
    {
        RuleFor(x => x.id)
            .NotEmpty().WithMessage("Work Order ID must not be empty.")
            .WithErrorCode("WorkOrderId_Is_Required")
            ;
    }
}
