using ErrorOr;
using MechanicDomain.Abstractions;

namespace MechanicDomain.RepairTasks.Parts;

public sealed class Part : AuditableEntity
{
    public string? Name { get; private set; }
    public decimal Cost { get; private set; }
    public int Quantity { get; private set; }
    private static int MaxCost => 1000;
    private static int MaxQuantity => 10;

    private Part()
    { }

    private Part(Guid id, string name, decimal cost, int quantity)
        : base(id)
    {
        Name = name;
        Cost = cost;
        Quantity = quantity;
    }

    public ErrorOr<Updated> Update(string? name, decimal cost, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name)) return PartErrors.NameRequired;
        if (cost <= 0 || cost > MaxCost) return PartErrors.CostInvalid;
        if (quantity <= 0 || quantity > MaxQuantity) return PartErrors.QuantityInvalid;

        Name = name.Trim();
        Cost = cost;
        Quantity = quantity;

        return Result.Updated;
    }

    public static ErrorOr<Part> Create(Guid id, string name, decimal cost, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name)) return PartErrors.NameRequired;
        if (cost <= 0 || cost > MaxCost) return PartErrors.CostInvalid;
        if (quantity <= 0 || quantity > MaxQuantity) return PartErrors.QuantityInvalid;

        return new Part(id, name.Trim(), cost, quantity);
    }
    //ToDO: Implement IClonable
    public Part Clone() => new Part(Id, Name, Cost, Quantity);

 
}
