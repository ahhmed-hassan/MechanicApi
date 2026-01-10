namespace MechanicApi.ContractDomainMapping;

public static class WorkOrderStateMapping
{
    public static MechanicDomain.WorkOrders.Enums.WorkOrderState ToDomainWorkOrderState(this MechanicContracts.Shared.WorkOrderState contractsWorkOrderState)
    {
        return contractsWorkOrderState switch
        {
            MechanicContracts.Shared.WorkOrderState.Scheduled => MechanicDomain.WorkOrders.Enums.WorkOrderState.Scheduled,
            MechanicContracts.Shared.WorkOrderState.InProgress => MechanicDomain.WorkOrders.Enums.WorkOrderState.InProgress,
            MechanicContracts.Shared.WorkOrderState.Completed => MechanicDomain.WorkOrders.Enums.WorkOrderState.Completed,
            MechanicContracts.Shared.WorkOrderState.Cancelled => MechanicDomain.WorkOrders.Enums.WorkOrderState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(contractsWorkOrderState), contractsWorkOrderState, null)
        };
    }
}
