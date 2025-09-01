
using ErrorOr;
using FluentValidation;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.AssignLabor;

public record AssignLaborCommand(Guid WokrOrder, Guid Labor): IRequest<ErrorOr<Updated>>; 

public sealed class AssignLaborCommandValidator:AbstractValidator<AssignLaborCommand>
{
    public AssignLaborCommandValidator()
    {
        RuleFor(x => x.WokrOrder)
            .NotEmpty()
            .WithErrorCode("WorkOrderId_Required")
            .WithMessage("WorkOrder is required");

        RuleFor(x => x.Labor)
            .NotEmpty()
            .WithErrorCode("LaborId_Required")
            .WithMessage("Labor Id is required");
    }
}


