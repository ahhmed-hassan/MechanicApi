using FluentValidation;

namespace MechanicApplication.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandValidator : AbstractValidator<UpdateRepairTaskCommand>
{
    public UpdateRepairTaskCommandValidator()
    {
        RuleFor(x => x.RepairTaskId)
            .NotEmpty().WithMessage("Repair Task ID is required required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x.LaborCost)
            .InclusiveBetween(1,10000).WithMessage("Labor cost must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Labor cost must not exceed 10,000.");

        RuleFor(x => x.EstimatedDurationInMins)
            .IsInEnum().WithMessage("Invalid duration selected");

        RuleForEach(x => x.CommandParts).SetValidator(new UpdateRepairTaskPartCommandValidator());
    }

}
