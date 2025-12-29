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

   
}
