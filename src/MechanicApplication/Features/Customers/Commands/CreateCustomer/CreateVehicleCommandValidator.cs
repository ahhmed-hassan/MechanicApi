using FluentValidation;
using FluentValidation.Validators;

namespace MechanicApplication.Features.Customers.Commands.CreateCustomer;

internal class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(50).WithMessage("Make cannot exceed 50 characters");
        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(50).WithMessage("Model cannot exceed 50 characters");
        RuleFor(x => x.Year)
            .InclusiveBetween(1886, DateTime.Now.Year).WithMessage("Year must be between 1886 and the current year");
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("License plate is required")
            .MaximumLength(10).WithMessage("License plate cannot exceed 10 characters");
    }
}