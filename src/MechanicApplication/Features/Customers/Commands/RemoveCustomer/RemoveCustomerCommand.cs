using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.Customers.Commands.RemoveCustomer;
public sealed record RemoveCustomerCommand(Guid CustomerId)
    : IRequest<ErrorOr<Deleted>>;
