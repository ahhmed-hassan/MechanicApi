using FluentValidation;

namespace MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandValidator : AbstractValidator<CreateRepairTaskCommand>
{
    public CreateRepairTaskCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");
        RuleFor(x => x.LaborCost)
            .GreaterThan(0).WithMessage("Labor cost must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("Labor cost must not exceed 10,000.");

        RuleFor(x =>x.EstimatedDurationInMins)
            .NotNull().WithMessage("Estimated duration is required.")
            .IsInEnum().WithMessage("Estimated duration must be a valid enum value.");

        RuleFor(x=>x.Parts)
            .NotNull().WithMessage("Parts list cannot be null.")
            .Must(parts => parts != null && parts.Count > 0).WithMessage("At least one part is required.");
        RuleForEach(x => x.Parts).SetValidator(new CreateRepairTaskPartCommandValidator());
    }

}
