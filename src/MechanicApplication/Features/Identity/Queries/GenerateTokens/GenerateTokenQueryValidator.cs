using FluentValidation;

namespace MechanicApplication.Features.Identity.Queries.GenerateTokens;

public sealed class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotNull().NotEmpty()
            .WithMessage("Email is required.")
            .WithErrorCode("Email_Null_Or_Empty")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotNull().NotEmpty()
            .WithMessage("Password is required.")
            .WithErrorCode("Password_Null_Or_Empty");
        
    }

}
