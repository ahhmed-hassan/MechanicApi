
using FluentValidation;

namespace MechanicApplication.Features.RepairTasks.Queries.GetRepiarTaskById
{
    public class GetRepairTaskByIdValidator : AbstractValidator<GetRepairTaskByIdQuery>
    {
        public GetRepairTaskByIdValidator()
        {
            RuleFor(x => x.RepairTaskId)
                .NotEmpty().WithMessage("Repair Task ID is required.")
                .Must(id => id != Guid.Empty).WithMessage("Repair Task ID must be a valid GUID.");
        }
    }
}
