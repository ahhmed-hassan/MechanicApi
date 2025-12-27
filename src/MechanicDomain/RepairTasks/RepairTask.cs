using ErrorOr;
using MechanicDomain.Abstractions;
using MechanicDomain.Customers.Vehicles;
using MechanicDomain.RepairTasks.Enums;
using MechanicDomain.RepairTasks.Parts;

namespace MechanicDomain.RepairTasks;

public sealed class RepairTask : AuditableEntity
{
    public string Name { get; private set; }
    public decimal LaborCost { get; private set; }
    public RepairDurationInMinutes EstimatedDurationInMins { get; private set; }


    public List<Part> Parts { get; } = new();
    public decimal TotalCost => LaborCost + Parts.Sum(p => p.Cost * p.Quantity);

#pragma warning disable CS8618

    private RepairTask()
    { }

#pragma warning restore CS8618

    private RepairTask(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins, List<Part> parts)
        : base(id)
    {
        Name = name;
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;
        Parts = parts?? new();
    }

    public static ErrorOr<RepairTask> Create(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins, List<Part> parts)
    {
        if (string.IsNullOrWhiteSpace(name)) return RepairTaskErrors.NameRequired;
        if (laborCost <= 0) return RepairTaskErrors.LaborCostInvalid;
        if (!Enum.IsDefined(estimatedDurationInMins)) return RepairTaskErrors.DurationInvalid;

        return new RepairTask(id, name.Trim(), laborCost, estimatedDurationInMins, parts);
    }

    public ErrorOr<Updated> UpsertParts(List<Part> incomingParts)
    {
        Parts.RemoveAll(existing => incomingParts.Any(p => p.Id == existing.Id));
        Parts.AddRange(incomingParts.Select(part => part.Clone()));
        return Result.Updated;
        //var existingIds = Parts.Select(p => p.Id).ToHashSet();
        //var existInParts = (Part v) => existingIds.Contains(v.Id);
        ////var groups = incomingParts.GroupBy(existInParts)
        //  //                .ToDictionary(g => g.Key, g => g.ToList());

        ////var existingParts = groups.ContainsKey(true) ? groups[true] : new List<Part>();
        ////var newParts = groups.ContainsKey(false) ? groups[false] : new List<Part>();

    }
    private static int MaxLaborCost => 10000;
    public ErrorOr<Updated> Update(string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins)
    {
        if (string.IsNullOrWhiteSpace(name)) return RepairTaskErrors.NameRequired;


        if (laborCost <= 0 || laborCost > MaxLaborCost) return RepairTaskErrors.LaborCostInvalid;


        if (!Enum.IsDefined(estimatedDurationInMins)) return RepairTaskErrors.DurationInvalid;


        Name = name.Trim();
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;

        return Result.Updated;
    }
}

