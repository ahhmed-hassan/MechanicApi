
using MechanicContracts.Shared;
using MechanicDomain.WorkOrders.Enums;

namespace MechanicContracts.Requests.RepairTasks;

public sealed record RelocateWorkOrderRequest(DateTimeOffset NewStart, Spot newSpot);

