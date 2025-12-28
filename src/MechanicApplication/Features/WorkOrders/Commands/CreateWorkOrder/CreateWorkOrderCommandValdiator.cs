using FluentValidation;
using MechanicDomain.Customers.Vehicles;
using System.Data;

namespace MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValdiator
    : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValdiator()
    {
     RuleFor(x=> x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId must not be empty");

    }
}
