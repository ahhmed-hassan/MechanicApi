
using MechanicDomain.Customers;
using MechanicDomain.Customers.Vehicles;
using MechanicDomain.Employees;
using MechanicDomain.Identity;
using MechanicDomain.RepairTasks;
using MechanicDomain.RepairTasks.Parts;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;

namespace MechanicApplication.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Customer> Customers { get; }
    public DbSet<Part> Parts { get; }
    public DbSet<RepairTask> RepairTasks { get; }
    public DbSet<Vehicle> Vehicles { get; }
    public DbSet<WorkOrder> WorkOrders { get; }
    public DbSet<Employee> Employees { get; }
    public DbSet<Invoice> Invoices { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
}
