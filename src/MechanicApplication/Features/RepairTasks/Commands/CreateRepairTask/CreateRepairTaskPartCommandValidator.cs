using FluentValidation;
using FluentValidation.Validators;

namespace MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskPartCommandValidator : AbstractValidator<CreateRepairTaskPaskPartCommand>
{
    public CreateRepairTaskPartCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Part name is required.")
            .MaximumLength(100).WithMessage("Part name must not exceed 100 characters.");
        RuleFor(x => x.Cost)
            .GreaterThan(0).WithMessage("Part cost must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Part cost must not exceed 1000.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Part quantity must be greater than 0.")
            .LessThanOrEqualTo(10).WithMessage("Part quantity must not exceed 10.");
    }
}