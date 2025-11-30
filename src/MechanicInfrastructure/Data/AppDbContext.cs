
using MechanicApplication.Common.Interfaces;
using MechanicDomain.Abstractions;
using MechanicDomain.Customers;
using MechanicDomain.Customers.Vehicles;
using MechanicDomain.Employees;
using MechanicDomain.Identity;
using MechanicDomain.RepairTasks;
using MechanicDomain.RepairTasks.Parts;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Billing;
using MechanicInfrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace MechanicInfrastructure.Data;

public class AppDbContext
    (
    DbContextOptions<AppDbContext> options,
    IMediator mediator
    )
    : IdentityDbContext<AppUser>(options), IAppDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Part> Parts => Set<Part>();

    public DbSet<RepairTask> RepairTasks => Set<RepairTask>();

    public DbSet<Vehicle> Vehicles => Set <Vehicle>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var domainEntities = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity { DomainEvents.Count: > 0 })
            .Select(e => (Entity)e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, ct);
        }
        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }
    }
}
