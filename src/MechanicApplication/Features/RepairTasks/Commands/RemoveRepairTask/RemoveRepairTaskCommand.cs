using ErrorOr;
using FluentValidation;
using MediatR;

namespace MechanicApplication.Features.RepairTasks.Commands.RemoveRepairTask;

public sealed record RemoveRepairTaskCommand(Guid RepairTaskId) : IRequest<ErrorOr<Deleted>>;

public class RemoveRepairTaskCommandValidator : AbstractValidator<RemoveRepairTaskCommand>
{
    public RemoveRepairTaskCommandValidator()
    {
        RuleFor(x => x.RepairTaskId)
            .NotEmpty().WithMessage("RepairTaskId is required.");
    }
}