using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Common;

[Collection(WebbAppFactoryColllection.CollectionName)]
public abstract class IntegrationTestsBase
    : IAsyncLifetime
{
    protected readonly WebAppFactory _factory;
    protected readonly IMediator _mediator;
    protected readonly IAppDbContext _dbContext;

    protected IntegrationTestsBase(WebAppFactory factory)
    {
        _factory = factory;
        _mediator = factory.Mediator;
        _dbContext = factory.DbContext;
    }

    // ===== RUNS BEFORE EACH TEST =====
    public virtual async Task InitializeAsync()
    {
        await ClearDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task ClearDatabaseAsync()
    {
        // Order: delete children before parents (FK constraints)
        //_dbContext.WorkOrders.RemoveRange(_dbContext.WorkOrders);
        //_dbContext.Invoices.RemoveRange(_dbContext.Invoices);
        //_dbContext.RefreshTokens.RemoveRange(_dbContext.RefreshTokens);
        //_dbContext.RepairTasks.RemoveRange(_dbContext.RepairTasks);
        //_dbContext.Parts.RemoveRange(_dbContext.Parts);
        //_dbContext.Vehicles.RemoveRange(_dbContext.Vehicles);
        //_dbContext.Employees.RemoveRange(_dbContext.Employees);
        //_dbContext.Customers.RemoveRange(_dbContext.Customers);
        await _dbContext.WorkOrders.ExecuteDeleteAsync();
        await _dbContext.RepairTasks.ExecuteDeleteAsync();
        await _dbContext.Vehicles.ExecuteDeleteAsync();
        await _dbContext.Employees.ExecuteDeleteAsync();
        await _dbContext.Customers.ExecuteDeleteAsync();
        await _dbContext.Parts.ExecuteDeleteAsync();
        await _dbContext.Invoices.ExecuteDeleteAsync();
        await _dbContext.RefreshTokens.ExecuteDeleteAsync();
        await _dbContext.SaveChangesAsync(CancellationToken.None);
    }

    protected bool IsEmptyDatabase()
    {
        return _dbContext.WorkOrders.Count() == 0
            && _dbContext.Invoices.Count() == 0
            && _dbContext.RefreshTokens.Count() == 0
            && _dbContext.RepairTasks.Count() == 0
            && _dbContext.Parts.Count() == 0
            && _dbContext.Vehicles.Count() == 0
            && _dbContext.Employees.Count() == 0
            && _dbContext.Customers.Count() == 0;
    }
}
