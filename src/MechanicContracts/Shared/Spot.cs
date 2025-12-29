using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicContracts.Shared;

public enum Spot
{
    A, 
    B, 
    C, 
    D
}

public static class SpotExtensions
{
    public static IEnumerable<Spot> GetAllSpots()
    {
        return Enum.GetValues(typeof(Spot)).Cast<Spot>();
    }

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
