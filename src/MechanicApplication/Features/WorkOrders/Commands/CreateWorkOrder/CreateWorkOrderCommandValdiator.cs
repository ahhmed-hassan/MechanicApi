using FluentValidation;
using MechanicDomain.Customers.Vehicles;
using System.Data;

namespace MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValdiator
    : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValdiator()
    {
        RuleFor(x => x.VehicleId)
               .NotEmpty()
               .WithMessage("VehicleId must not be empty");

        RuleFor(x => x.RepairTasksIds)
                .NotEmpty()
                .WithMessage("RepairTasksIds must contain at least one item");
        
        RuleFor(x => x.StartAt)
            .GreaterThan(DateTimeOffset.Now)
            .WithMessage("StartAt must be in the future");

        RuleFor(x => x.LaborId)
            .NotEmpty()
            .WithMessage("LaborId must not be empty");

    }
}
