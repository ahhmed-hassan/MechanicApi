
using MechanicDomain.Employees;

namespace MechanicApplication.Features.Labors.DTOs;

public class LaborDTO
{
    public Guid LaborId { get; set; }
    public string Name { get; set; } = string.Empty;

    public static LaborDTO fromEmployee (Employee labor) =>
        new LaborDTO
        {
            LaborId = labor.Id,
            Name = labor.FullName
        };
    static public List<LaborDTO> fromEmployees (IEnumerable<Employee> labors) =>
        labors.Select(fromEmployee).ToList();
}
