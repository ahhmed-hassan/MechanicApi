
using MechanicContracts.Shared;

namespace MechanicContracts.Requests.WorkOrders;

public sealed record UpdateWorkOrderStateRequest (WorkOrderState State); 
