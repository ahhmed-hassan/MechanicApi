using MechanicContracts.Shared;

namespace MechanicContracts.Requests.WorkOrders;

public sealed record RelocateWorkOrderRequest(DateTimeOffset NewStart, Spot newSpot);

