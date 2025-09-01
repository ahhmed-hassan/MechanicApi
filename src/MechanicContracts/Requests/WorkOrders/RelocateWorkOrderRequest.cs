using MechanicContracts.Shared;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicContracts.Requests.WorkOrders;

public sealed record RelocateWorkOrderRequest(DateTimeOffset NewStart, Spot newSpot);

