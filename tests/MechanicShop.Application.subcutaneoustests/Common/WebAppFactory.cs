
using MechanicApplication.Common.Interfaces;
using MechanicInfrastructure.Data.Migrations;
using MechanicInfrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using Testcontainers.MsSql;
using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Common;

public class WebAppFactory : WebApplicationFactory<AssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        // .WithPassword("yourStrong(!)Password")
        .Build();

    public IMediator Mediator => Services
        .CreateScope()
        .ServiceProvider
        .GetRequiredService<IMediator>();

    public IAppDbContext DbContext => Services
        .CreateScope()
        .ServiceProvider
        .GetRequiredService<IAppDbContext>();
    Task IAsyncLifetime.InitializeAsync()
    {
        return _dbContainer.StartAsync()
          .ContinueWith(async _ =>
          {
              using var scope = Services.CreateScope();
              var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

              context.WorkOrders.RemoveRange(context.WorkOrders);
              await context.SaveChangesAsync(default);
          }).Unwrap();
    }

    Task IAsyncLifetime.DisposeAsync() => _dbContainer.DisposeAsync().AsTask();

    /// <summary>
    /// Customize the test host used by the WebApplicationFactory.
    /// </summary>
    /// <remarks>
    /// This method runs in the test host creation pipeline and uses the TestHost-specific
    /// __ConfigureTestServices__ extension to alter the application's service registrations
    /// for testing. Changes are scoped to the TestServer created by the factory and do not
    /// modify the application's production configuration.
    ///
    /// Key actions performed here:
    /// 1) Remove all IHostedService registrations to prevent background services from starting during tests.
    /// 2) Remove existing EF Core DbContextOptions<AppDbContext> registrations and re-register AppDbContext
    ///    so it uses the ephemeral Testcontainers SQL Server instance. Any registered
    ///    ISaveChangesInterceptor implementations are preserved and attached to the DbContext.
    /// 3) Remove AppSettings registrations (the app's configured options) and then apply a
    ///    PostConfigure override to set deterministic OpeningTime and ClosingTime values for tests.
    /// </remarks>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove background/hosted services so they do not run during tests (timers, cleaners, etc.)
            services.RemoveAll<IHostedService>();
            //services.RemoveAll<OverdueBookingCleanupService>();

            // Remove existing EF Core options so we can reconfigure the AppDbContext for tests.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                // Preserve any save-change interceptors already registered in DI and add them to the DbContext.
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                // Point EF Core to the Testcontainers SQL Server instance for integration tests.
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            // Remove AppSettings option registrations so tests can override them explicitly.
            services.RemoveAll<AppSettings>();

            // Explicit override AFTER Configure: PostConfigure runs after normal options setup,
            // ensuring the test values take precedence for the lifetime of the test host.
            services.PostConfigure<AppSettings>(opts =>
            {
                opts.OpeningTime = new TimeOnly(9, 0);
                opts.ClosingTime = new TimeOnly(18, 0);
            });
        });
    }

}
