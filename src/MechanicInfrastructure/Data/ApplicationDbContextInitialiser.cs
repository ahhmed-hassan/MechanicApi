using MechanicDomain.Customers;
using MechanicDomain.Customers.Vehicles;
using MechanicDomain.Employees;
using MechanicDomain.Identity;
using MechanicDomain.RepairTasks;
using MechanicDomain.RepairTasks.Enums;
using MechanicDomain.RepairTasks.Parts;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Enums;
using MechanicInfrastructure.Data.Migrations;
using MechanicInfrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MechanicInfrastructure.Data;

/// <summary>
/// seeds the database with initial data
/// </summary>
/// 
/// <param name="logger"></param>
/// <param name="context"> the database context </param>
/// <param name="userManager"> The manager for all the users for adding the <see cref="AppUser"/> to the database or checking if they even exist</param>
/// <param name="roleManager"> For the manager and labor roles</param>
/// <remarks>
/// See after the code for example data used for seeding
/// </remarks>
public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    AppDbContext context, UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var managerRole = new IdentityRole(nameof(Role.Manager));

        if (_roleManager.Roles.All(r => r.Name != managerRole.Name))
        {
            await _roleManager.CreateAsync(managerRole);
        }

        var laborRole = new IdentityRole(nameof(Role.Labor));

        if (_roleManager.Roles.All(r => r.Name != laborRole.Name))
        {
            await _roleManager.CreateAsync(laborRole);
        }

        // Default users
        var manager = new AppUser
        {
            Id = "19a59129-6c20-417a-834d-11a208d32d96",
            Email = "pm@localhost",
            UserName = "pm@localhost",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.Email != manager.Email))
        {
            await _userManager.CreateAsync(manager, manager.Email);

            if (!string.IsNullOrWhiteSpace(managerRole.Name))
            {
                await _userManager.AddToRolesAsync(manager, [managerRole.Name]);
            }
        }

        var labor01 = new AppUser
        {
            Id = "b6327240-0aea-46fc-863a-777fc4e42560",
            Email = "john.labor@localhost",
            UserName = "john.labor@localhost",
            EmailConfirmed = true
        };


        var labor02 = new AppUser
        {
            Id = "8104ab20-26c2-4651-b1de-c0baf04dbbd9",
            Email = "peter.labor@localhost",
            UserName = "peter.labor@localhost",
            EmailConfirmed = true
        };

       var labor03 = new AppUser
        {
            Id = "e17c83de-1089-4f19-bf79-5f789133d37f",
            Email = "kevin.labor@localhost",
            UserName = "kevin.labor@localhost",
            EmailConfirmed = true
        };

        var labor04 = new AppUser
        {
            Id = "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
            Email = "suzan.labor@localhost",
            UserName = "suzan.labor@localhost",
            EmailConfirmed = true
        };
        List<AppUser> laborObjects = new() {labor01, labor02, labor03, labor04};
        AppUser[] laborsAndEmployee = [..laborObjects, manager];

        foreach (AppUser labor in laborObjects)
        {
            if (_userManager.Users.All(u => u.Email != labor.Email))
            {
                await _userManager.CreateAsync(labor, labor.Email);
                if (!string.IsNullOrWhiteSpace(laborRole.Name))
                {
                    await _userManager.AddToRolesAsync(labor, [laborRole.Name]);
                }
            }
        }

        if (!_context.Employees.Any())
        {
            _context.Employees.AddRange(
            [
                Employee.Create(Guid.Parse(manager.Id), "Primary", "Manager", Role.Manager).Value,
                Employee.Create(Guid.Parse(labor01.Id), "John", "S.", Role.Labor).Value,
                Employee.Create(Guid.Parse(labor02.Id), "Peter", "R.", Role.Labor).Value,
                Employee.Create(Guid.Parse(labor03.Id), "Kevin", "M.", Role.Labor).Value,
                Employee.Create(Guid.Parse(labor04.Id), "Suzan", "L.", Role.Labor).Value
            ]);
        }

        if (!_context.Customers.Any())
        {
            List<Vehicle> vehicles = [
                        Vehicle.Create(id: Guid.Parse("61401e63-007b-4b1c-8914-9eb6e9bd95c5"), make: "Toyota", model: "Camry", year: 2020, licensePlate: "ABC123").Value,
                        Vehicle.Create(id: Guid.Parse("13c80914-41ad-4d46-b7bb-60f6c89ad01e"), make: "Honda", model: "Civic", year: 2018, licensePlate: "XYZ456").Value,
                    ];

            _context.Customers.AddRange(
            [
                Customer.Create(id: Guid.Parse("f522bbe5-e3b1-4e2c-a8a3-c41550dcf39d"), name: "John Doe", phoneNumber: "123456789", email: "john.doe@localhost", vehicles: vehicles).Value,
                Customer.Create(id: Guid.Parse("73a04dd3-c81a-4a54-9882-ef1017eb192d"), name: "Sarah Peter", phoneNumber: "987654321", email: "sarah.peter@localhost", vehicles: [Vehicle.Create(id: Guid.Parse("a04f329d-0f5a-46a0-beae-699c034ae401"), make: "Ford", model: "Focus", year: 2021, licensePlate: "DEF789").Value, Vehicle.Create(id: Guid.Parse("cf60e95b-5752-4c26-aa07-31a34164606c"), make: "Chevrolet", model: "Malibu", year: 2019, licensePlate: "GHI012").Value,]).Value,
            ]);
        }

        if (!_context.RepairTasks.Any())
        {
            _context.RepairTasks.AddRange([
                RepairTask.Create(id: Guid.Parse("616aebb1-d515-4b40-8d47-8d5c0b67a313"), name: "Engine Oil Change", laborCost: 50.00m, estimatedDurationInMins: RepairDurationInMinutes.Min60, parts: [Part.Create(Guid.Parse("ec65225c-9066-4a1c-974f-f183c39fdd16"), "Engine Oil", 25.00m, 1).Value, Part.Create(Guid.Parse("62ad80e3-2cff-41af-ab40-16fab8db8b38"), "Oil Filter", 10.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("4fa0be55-06f6-4616-b086-e1f0c9354cd8"), name: "Brake Replacement", laborCost: 150.00m, estimatedDurationInMins: RepairDurationInMinutes.Min90, parts: [Part.Create(Guid.Parse("86375a12-715e-4aa4-aad9-c0f9ccf44a14"), "Brake Pads", 40.00m, 2).Value, Part.Create(Guid.Parse("526d89c3-a971-4ea7-ba15-de6b50b13c21"), "Brake Fluid", 15.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("a376b5d1-6b2d-4dd8-883e-d3d1721c1316"), name: "Tire Rotation", laborCost: 30.00m, estimatedDurationInMins: RepairDurationInMinutes.Min45, parts: [Part.Create(Guid.Parse("a46f974e-a198-4098-8a1f-6be6e68ec743"), "Tire Valve", 5.00m, 4).Value]).Value,
                RepairTask.Create(id: Guid.Parse("a770cc6e-0c8b-4ac5-9ee6-6928682bd47e"), name: "Battery Replacement", laborCost: 70.00m, estimatedDurationInMins: RepairDurationInMinutes.Min30, parts: [Part.Create(Guid.Parse("d4fd3255-29dc-4d45-9d87-f58ab98bc28b"), "Car Battery", 120.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("e4c2b675-4a60-488f-a7b4-61966e7e80e3"), name: "Wheel Alignment", laborCost: 80.00m, estimatedDurationInMins: RepairDurationInMinutes.Min60, parts: [Part.Create(Guid.Parse("fa3b9a7e-1c2d-4e3f-9b8a-0c1d2e3f4a5b"), "Alignment Shim Kit (per wheel)", 5.00m, 4).Value]).Value,
                RepairTask.Create(id: Guid.Parse("1cb1608c-3bc7-4325-99c3-8244c0fb412f"), name: "Air Conditioning Recharge", laborCost: 100.00m, estimatedDurationInMins: RepairDurationInMinutes.Min30, parts: [Part.Create(Guid.Parse("526dca0a-d236-47d3-8e8f-c83d555b2de9"), "Refrigerant", 50.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("a8e9b4e0-8581-40df-967d-51a0f4fabc0e"), name: "Spark Plug Replacement", laborCost: 90.00m, estimatedDurationInMins: RepairDurationInMinutes.Min60, parts: [Part.Create(Guid.Parse("019f5eab-a8a5-44d4-92b3-1f998e3f10c2"), "Spark Plug", 10.00m, 4).Value]).Value,
                RepairTask.Create(id: Guid.Parse("90f2f3ef-3357-439e-9689-628aa08200c1"), name: "Engine Diagnostic", laborCost: 120.00m, estimatedDurationInMins: RepairDurationInMinutes.Min120, parts: [Part.Create(Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"), "Smoke Leak Detector Fluid Cartridge", 20.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("d124651e-ca72-467e-ba28-81ea4a2080bc"), name: "Timing Belt Replacement", laborCost: 200.00m, estimatedDurationInMins: RepairDurationInMinutes.Min120, parts: [Part.Create(Guid.Parse("06b764a0-73a2-4c37-b279-adae3856499c"), "Timing Belt", 75.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("cee9b309-8620-4028-8d38-2532771ab3ea"), name: "Transmission Fluid Change", laborCost: 100.00m, estimatedDurationInMins: RepairDurationInMinutes.Min45, parts: [Part.Create(Guid.Parse("0a8b0c19-873a-4da0-811b-45ff85bca0ed"), "Transmission Fluid", 60.00m, 1).Value]).Value
            ]);
        }

        await _context.SaveChangesAsync();

        if (!_context.WorkOrders.Any())
        {
            var repairTasks = _context.RepairTasks.ToList();
            var vehicles = _context.Vehicles.ToList();
            string[] laborIds = [..laborObjects.Select(x=>x.Id)];
            Spot[] spots = [Spot.A, Spot.B, Spot.C, Spot.D];

            var generatedWorkOrders = new List<WorkOrder>();
            Random random = new();
            DateTimeOffset startDate = DateTimeOffset.Now.Date.AddDays(1); // Start from tomorrow
            DateTimeOffset endDate = startDate.AddMonths(1); // Generate for the next month
            TimeSpan openTime = TimeSpan.FromHours(12);  // 9:00 am utc -4
            TimeSpan closeTime = TimeSpan.FromHours(23); // 18:00 am utc -4
            int totalMinutes = (int)(closeTime - openTime).TotalMinutes;

            while (startDate < endDate)
            {
                foreach (Spot spot in spots)
                {
                    int occupiedMinutes = 0;
                    int minOccupancy = (int)(totalMinutes * 0.6); // Minimum 60% usage
                    int maxOccupancy = (int)(totalMinutes * 0.8); // Maximum 80% usage
                    List<WorkOrder> spotWorkOrders = [];

                    DateTimeOffset currentTime = startDate.Add(openTime);

                    while (occupiedMinutes < minOccupancy && currentTime.TimeOfDay < closeTime)
                    {
                        var selectedTask = repairTasks
                                            .DistinctBy(t => t.Id)
                                            .OrderBy(_ => Guid.NewGuid())
                                            .Take(Random.Shared.Next(1, Math.Min(4, repairTasks.Select(t => t.Id).Distinct().Count())))
                                            .ToList();
                        var laborId = laborIds[random.Next(laborIds.Length)];
                        var duration = selectedTask.Sum(st => (int)st.EstimatedDurationInMins);

                        if (occupiedMinutes + duration > maxOccupancy)
                        {
                            break;
                        }

                        DateTimeOffset startAt = currentTime;
                        DateTimeOffset endAt = startAt.AddMinutes(duration);

                        var availableVehicle = vehicles
                        .Where(v => !generatedWorkOrders.Any(w =>
                            w.VehicleId == v.Id &&
                            w.StartAtUtc.Date == startAt.Date &&
                            w.StartAtUtc < endAt &&
                            w.EndAtUtc > startAt))
                        .OrderBy(_ => Guid.NewGuid())
                        .FirstOrDefault();

                        if (availableVehicle == null)
                        {
                            break;
                        }

                        if (endAt.TimeOfDay > closeTime)
                        {
                            break;
                        }

                        var workOrder = WorkOrder.Create(
                            Guid.NewGuid(),
                            availableVehicle.Id,
                            startAt,
                            Guid.Parse(laborId),
                            spot,
                            selectedTask);

                        spotWorkOrders.Add(workOrder.Value);
                        occupiedMinutes += duration;

                        currentTime = startDate.Add(openTime).AddMinutes(occupiedMinutes);
                    }

                    // Ensure at least 60% occupancy is reached for this spot before adding WorkOrders
                    if (occupiedMinutes >= minOccupancy)
                    {
                        generatedWorkOrders.AddRange(spotWorkOrders);
                    }
                }

                startDate = startDate.AddDays(1);
            }

            var repairTasksForFirstOrder = _context.RepairTasks
             .OrderBy(_ => Guid.NewGuid())
             .Take(2)
             .ToList();

            var utcNow = DateTimeOffset.UtcNow;

            // Round down to nearest 30-minute block
            var floored = new DateTimeOffset(
                  utcNow.Year,
                  utcNow.Month,
                  utcNow.Day,
                  utcNow.Hour,
                  utcNow.Minute - (utcNow.Minute % 15),
                  0,
                  TimeSpan.Zero); // keep in UTC

            var startTimeFirstOrder = floored;

            var workOrderStartingNow = WorkOrder.Create(
                Guid.NewGuid(),
                _context.Vehicles.OrderBy(_ => Guid.NewGuid()).First().Id,
                startTimeFirstOrder,
                Guid.Parse(labor01.Id),
                Spot.A,
                repairTasksForFirstOrder).Value;

            workOrderStartingNow.UpdateState(WorkOrderState.InProgress);

            var repairTasksEndingNow = _context.RepairTasks
      .First(rt => rt.EstimatedDurationInMins == RepairDurationInMinutes.Min60);

            // Align to 15-minute slot: started 45 minutes ago
            var startedAgo = utcNow.AddMinutes(-45);
            var roundedStart = new DateTimeOffset(
                startedAgo.Year,
                startedAgo.Month,
                startedAgo.Day,
                startedAgo.Hour,
                startedAgo.Minute - (startedAgo.Minute % 15),
                0,
                TimeSpan.Zero);


            WorkOrder value = WorkOrder.Create(
                Guid.NewGuid(),
                _context.Vehicles.OrderBy(_ => Guid.NewGuid()).First().Id,
                roundedStart,
                Guid.Parse(labor02.Id),
                Spot.B,
                [repairTasksEndingNow])
            .Value;
            var workOrderEndingNow = value;

            workOrderEndingNow.UpdateState(WorkOrderState.InProgress);

            generatedWorkOrders.AddRange(workOrderStartingNow, workOrderEndingNow);

            _context.WorkOrders.AddRange(generatedWorkOrders);

            await _context.SaveChangesAsync();
        }
    }
}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

// Example Employees data for seeding (_context.Employees):
//"Employees" : [
//  { "UserId": "19a59129-6c20-417a-834d-11a208d32d96", "FirstName": "Primary", "LastName": "Manager", "Role": "Manager" },
//  { "UserId": "b6327240-0aea-46fc-863a-777fc4e42560", "FirstName": "John",    "LastName": "S.", "Role": "Labor" },
//  { "UserId": "8104ab20-26c2-4651-b1de-c0baf04dbbd9", "FirstName": "Peter",  "LastName": "R.", "Role": "Labor" },
//  { "UserId": "e17c83de-1089-4f19-bf79-5f789133d37f", "FirstName": "Kevin",  "LastName": "M.", "Role": "Labor" },
//  { "UserId": "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c", "FirstName": "Suzan",  "LastName": "L.", "Role": "Labor" }
//]

// Example Customers data for seeding _context.Customers):
//"Customers" :[
//  {
//    "Id": "f522bbe5-e3b1-4e2c-a8a3-c41550dcf39d",
//    "Name": "John Doe",
//    "Phone": "123456789",
//    "Email": "john.doe@localhost",
//    "Vehicles": [
//      { "Id": "61401e63-007b-4b1c-8914-9eb6e9bd95c5", "Make": "Toyota", "Model": "Camry", "Year": 2020, "License": "ABC123" },
//      { "Id": "13c80914-41ad-4d46-b7bb-60f6c89ad01e", "Make": "Honda",  "Model": "Civic", "Year": 2018, "License": "XYZ456" }
//    ]
//  },
//  {
//    "Id": "73a04dd3-c81a-4a54-9882-ef1017eb192d",
//    "Name": "Sarah Peter",
//    "Phone": "987654321",
//    "Email": "sarah.peter@localhost",
//    "Vehicles": [
//      { "Id": "a04f329d-0f5a-46a0-beae-699c034ae401", "Make": "Ford",      "Model": "Focus",   "Year": 2021, "License": "DEF789" },
//      { "Id": "cf60e95b-5752-4c26-aa07-31a34164606c", "Make": "Chevrolet", "Model": "Malibu",   "Year": 2019, "License": "GHI012" }
//    ]
//  }
//]


// Example RepairTasks data for seeding:
//_context.RepairTasks:
//"RepairTasks" : [
//  {
//    "Id": "616aebb1-d515-4b40-8d47-8d5c0b67a313",
//    "Name": "Engine Oil Change",
//    "LaborCost": 50.00,
//    "EstimatedDurationInMins": "Min60",
//    "Parts": [
//      { "Id": "ec65225c-9066-4a1c-974f-f183c39fdd16", "Name": "Engine Oil", "Cost": 25.00, "Qty": 1 },
//      { "Id": "62ad80e3-2cff-41af-ab40-16fab8db8b38", "Name": "Oil Filter", "Cost": 10.00, "Qty": 1 }
//    ]
//  },
//  {
//    "Id": "4fa0be55-06f6-4616-b086-e1f0c9354cd8",
//    "Name": "Brake Replacement",
//    "LaborCost": 150.00,
//    "EstimatedDurationInMins": "Min90",
//    "Parts": [
//      { "Id": "86375a12-715e-4aa4-aad9-c0f9ccf44a14", "Name": "Brake Pads", "Cost": 40.00, "Qty": 2 },
//      { "Id": "526d89c3-a971-4ea7-ba15-de6b50b13c21", "Name": "Brake Fluid", "Cost": 15.00, "Qty": 1 }
//    ]
//  },
//  {
//    "Id": "a376b5d1-6b2d-4dd8-883e-d3d1721c1316",
//    "Name": "Tire Rotation",
//    "LaborCost": 30.00,
//    "EstimatedDurationInMins": "Min45",
//    "Parts": [
//      { "Id": "a46f974e-a198-4098-8a1f-6be6e68ec743", "Name": "Tire Valve", "Cost": 5.00, "Qty": 4 }
//    ]
//  },
//  {
//    "Id": "a770cc6e-0c8b-4ac5-9ee6-6928682bd47e",
//    "Name": "Battery Replacement",
//    "LaborCost": 70.00,
//    "EstimatedDurationInMins": "Min30",
//    "Parts": [
//      { "Id": "d4fd3255-29dc-4d45-9d87-f58ab98bc28b", "Name": "Car Battery", "Cost": 120.00, "Qty": 1 }
//    ]
//  },
//  {
//    "Id": "e4c2b675-4a60-488f-a7b4-61966e7e80e3",
//    "Name": "Wheel Alignment",
//    "LaborCost": 80.00,
//    "EstimatedDurationInMins": "Min60",
//    "Parts": [
//      { "Id": "fa3b9a7e-1c2d-4e3f-9b8a-0c1d2e3f4a5b", "Name": "Alignment Shim Kit (per wheel)", "Cost": 5.00, "Qty": 4 }
//    ]
//  },
//  {
//    "Id": "1cb1608c-3bc7-4325-99c3-8244c0fb412f",
//    "Name": "Air Conditioning Recharge",
//    "LaborCost": 100.00,
//    "EstimatedDurationInMins": "Min30",
//    "Parts": [
//      { "Id": "526dca0a-d236-47d3-8e8f-c83d555b2de9", "Name": "Refrigerant", "Cost": 50.00, "Qty": 1 }
//    ]
//  },
//  {
//    "Id": "a8e9b4e0-8581-40df-967d-51a0f4fabc0e",
//    "Name": "Spark Plug Replacement",
//    "LaborCost": 90.00,
//    "EstimatedDurationInMins": "Min60",
//    "Parts": [
//      { "Id": "019f5eab-a8a5-44d4-92b3-1f998e3f10c2", "Name": "Spark Plug", "Cost": 10.00, "Qty": 4 }
//    ]
//  },
//  {
//    "Id": "90f2f3ef-3357-439e-9689-628aa08200c1",
//    "Name": "Engine Diagnostic",
//    "LaborCost": 120.00,
//    "EstimatedDurationInMins": "Min120",
//    "Parts": [
//      { "Id": "c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f", "Name": "Smoke Leak Detector Fluid Cartridge", "Cost": 20.00, "Qty": 1 }
//    ]
//  },
//  {
//    "Id": "d124651e-ca72-467e-ba28-81ea4a2080bc",
//    "Name": "Timing Belt Replacement",
//    "LaborCost": 200.00,
//    "EstimatedDurationInMins": "Min120",
//    "Parts": [
//      { "Id": "06b764a0-73a2-4c37-b279-adae3856499c", "Name": "Timing Belt", "Cost": 75.00, "Qty": 1 }
//    ]
//  },
//  {
//    "Id": "cee9b309-8620-4028-8d38-2532771ab3ea",
//    "Name": "Transmission Fluid Change",
//    "LaborCost": 100.00,
//    "EstimatedDurationInMins": "Min45",
//    "Parts": [
//      { "Id": "0a8b0c19-873a-4da0-811b-45ff85bca0ed", "Name": "Transmission Fluid", "Cost": 60.00, "Qty": 1 }
//    ]
//  }
//]


// Example WorkOrders data for seeding (_context.WorkOrders)
// Please refer to the TrySeedAsync method above for dynamically generated WorkOrders data, the here represents only static examples.
//:
//{
//  "WorkOrdersPreview": [
//    {
//      "Id": "2f8b6c58-0001-4b21-9d3a-111111111111",
//      "VehicleId": "61401e63-007b-4b1c-8914-9eb6e9bd95c5",
//      "Vehicle": { "Make": "Toyota", "Model": "Camry", "LicensePlate": "ABC123" },
//      "StartAtUtc": "2025-12-24T15:30:00Z",
//      "EndAtUtc": "2025-12-24T17:15:00Z",
//      "LaborId": "b6327240-0aea-46fc-863a-777fc4e42560",
//      "LaborEmail": "john.labor@localhost",
//      "Spot": "A",
//      "RepairTasks": [
//        { "Id": "616aebb1-d515-4b40-8d47-8d5c0b67a313", "Name": "Engine Oil Change", "DurationMins": 60, "LaborCost": 50.00 },
//        { "Id": "a376b5d1-6b2d-4dd8-883e-d3d1721c1316", "Name": "Tire Rotation", "DurationMins": 45, "LaborCost": 30.00 }
//      ],
//      "State": "InProgress",
//      "Source": "deterministic-now-order"
//    },
//    {
//    "Id": "4a9d7f39-0002-4d55-b3c3-222222222222",
//      "VehicleId": "13c80914-41ad-4d46-b7bb-60f6c89ad01e",
//      "Vehicle": { "Make": "Honda", "Model": "Civic", "LicensePlate": "XYZ456" },
//      "StartAtUtc": "2025-12-24T14:45:00Z",
//      "EndAtUtc": "2025-12-24T15:45:00Z",
//      "LaborId": "8104ab20-26c2-4651-b1de-c0baf04dbbd9",
//      "LaborEmail": "peter.labor@localhost",
//      "Spot": "B",
//      "RepairTasks": [
//        { "Id": "e4c2b675-4a60-488f-a7b4-61966e7e80e3", "Name": "Wheel Alignment", "DurationMins": 60, "LaborCost": 80.00 }
//      ],
//      "State": "InProgress",
//      "Source": "deterministic-ending-now"
//    },

//    // Representative generated orders for the next days (randomized in real seed)
//    {
//    "Id": "7b3c0001-0003-4a6b-8a8a-333333333333",
//      "VehicleId": "a04f329d-0f5a-46a0-beae-699c034ae401",
//      "Vehicle": { "Make": "Ford", "Model": "Focus", "LicensePlate": "DEF789" },
//      "StartAtUtc": "2025-12-25T12:00:00Z",
//      "EndAtUtc": "2025-12-25T13:30:00Z",
//      "LaborId": "e17c83de-1089-4f19-bf79-5f789133d37f",
//      "LaborEmail": "kevin.labor@localhost",
//      "Spot": "A",
//      "RepairTasks": [
//        { "Id": "4fa0be55-06f6-4616-b086-e1f0c9354cd8", "Name": "Brake Replacement", "DurationMins": 90, "LaborCost": 150.00 }
//      ],
//      "State": "Scheduled",
//      "Source": "generated"
//    },
//    {
//    "Id": "8c1d0002-0004-4b6c-9b9b-444444444444",
//      "VehicleId": "cf60e95b-5752-4c26-aa07-31a34164606c",
//      "Vehicle": { "Make": "Chevrolet", "Model": "Malibu", "LicensePlate": "GHI012" },
//      "StartAtUtc": "2025-12-25T13:30:00Z",
//      "EndAtUtc": "2025-12-25T14:00:00Z",
//      "LaborId": "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
//      "LaborEmail": "suzan.labor@localhost",
//      "Spot": "A",
//      "RepairTasks": [
//        { "Id": "a770cc6e-0c8b-4ac5-9ee6-6928682bd47e", "Name": "Battery Replacement", "DurationMins": 30, "LaborCost": 70.00 }
//      ],
//      "State": "Scheduled",
//      "Source": "generated"
//    },
//    {
//    "Id": "9d2e0003-0005-4c7d-0c0c-555555555555",
//      "VehicleId": "61401e63-007b-4b1c-8914-9eb6e9bd95c5",
//      "Vehicle": { "Make": "Toyota", "Model": "Camry", "LicensePlate": "ABC123" },
//      "StartAtUtc": "2025-12-26T12:00:00Z",
//      "EndAtUtc": "2025-12-26T13:00:00Z",
//      "LaborId": "b6327240-0aea-46fc-863a-777fc4e42560",
//      "LaborEmail": "john.labor@localhost",
//      "Spot": "C",
//      "RepairTasks": [
//        { "Id": "616aebb1-d515-4b40-8d47-8d5c0b67a313", "Name": "Engine Oil Change", "DurationMins": 60, "LaborCost": 50.00 }
//      ],
//      "State": "Scheduled",
//      "Source": "generated"
//    },
//    {
//    "Id": "a3f40004-0006-4d8e-1d1d-666666666666",
//      "VehicleId": "13c80914-41ad-4d46-b7bb-60f6c89ad01e",
//      "Vehicle": { "Make": "Honda", "Model": "Civic", "LicensePlate": "XYZ456" },
//      "StartAtUtc": "2025-12-27T16:15:00Z",
//      "EndAtUtc": "2025-12-27T18:15:00Z",
//      "LaborId": "8104ab20-26c2-4651-b1de-c0baf04dbbd9",
//      "LaborEmail": "peter.labor@localhost",
//      "Spot": "D",
//      "RepairTasks": [
//        { "Id": "90f2f3ef-3357-439e-9689-628aa08200c1", "Name": "Engine Diagnostic", "DurationMins": 120, "LaborCost": 120.00 }
//      ],
//      "State": "Scheduled",
//      "Source": "generated"
//    },
//    {
//    "Id": "b4g50005-0007-4e9f-2e2e-777777777777",
//      "VehicleId": "a04f329d-0f5a-46a0-beae-699c034ae401",
//      "Vehicle": { "Make": "Ford", "Model": "Focus", "LicensePlate": "DEF789" },
//      "StartAtUtc": "2025-12-28T12:30:00Z",
//      "EndAtUtc": "2025-12-28T14:15:00Z",
//      "LaborId": "e17c83de-1089-4f19-bf79-5f789133d37f",
//      "LaborEmail": "kevin.labor@localhost",
//      "Spot": "B",
//      "RepairTasks": [
//        { "Id": "a376b5d1-6b2d-4dd8-883e-d3d1721c1316", "Name": "Tire Rotation", "DurationMins": 45, "LaborCost": 30.00 },
//        { "Id": "cee9b309-8620-4028-8d38-2532771ab3ea", "Name": "Transmission Fluid Change", "DurationMins": 45, "LaborCost": 100.00 }
//      ],
//      "State": "Scheduled",
//      "Source": "generated"
//    }
//  ]
//}