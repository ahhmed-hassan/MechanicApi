using ErrorOr;
using MechanicDomain.RepairTasks.Parts;

namespace MechanicShop.Tests.Common.RepaireTasks;

public static class PartFactory
{
    public static ErrorOr<Part> CreatePart(Guid? id = null, string name = "Brake Pad", decimal cost = 100m, int quantity = 2)
    {
        return Part.Create(
            id ?? Guid.NewGuid(),
            name,
            cost,
            quantity);
    }
}