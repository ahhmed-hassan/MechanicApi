using MechanicDomain.Abstractions;

namespace MechanicDomain.WorkOrders.Events;
public sealed record WorkOrderCompleted(Guid WorkOrderId) : DomainEvent; 