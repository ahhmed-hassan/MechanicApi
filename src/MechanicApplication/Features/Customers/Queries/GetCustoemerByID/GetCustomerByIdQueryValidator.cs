using FluentValidation;

namespace MechanicApplication.Features.Customers.Queries.GetCustoemerByID;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty()
            .WithErrorCode("CustomerId_Is_Required")
            .WithMessage("CustomerId is required.");
    }
}
