using MechanicContracts.Shared;

namespace MechanicApi.ContractDomainMapping;

public static class ContractsSpotToDomainSpot
{
    public static MechanicDomain.WorkOrders.Enums.Spot ToDomainSpot(this Spot contractsSpot)
    {
        return contractsSpot switch
        {
            Spot.A => MechanicDomain.WorkOrders.Enums.Spot.A,
            Spot.B => MechanicDomain.WorkOrders.Enums.Spot.B,
            Spot.C => MechanicDomain.WorkOrders.Enums.Spot.C,
            Spot.D => MechanicDomain.WorkOrders.Enums.Spot.D,
            _ => throw new ArgumentOutOfRangeException(nameof(contractsSpot), contractsSpot, null)
        };
    }
}
