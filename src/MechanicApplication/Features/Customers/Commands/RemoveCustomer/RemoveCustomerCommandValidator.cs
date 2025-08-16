using FluentValidation;

namespace MechanicApplication.Features.Customers.Commands.RemoveCustomer;

public sealed class RemoveCustomerCommandValidator : AbstractValidator<RemoveCustomerCommand>
{
    public RemoveCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID cannot be empty.");
    }
}
