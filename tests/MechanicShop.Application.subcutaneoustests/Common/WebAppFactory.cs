
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            //services.RemoveAll<OverdueBookingCleanupService>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            services.RemoveAll<AppSettings>();

            // Explicit override AFTER Configure
            services.PostConfigure<AppSettings>(opts =>
            {
                opts.OpeningTime = new TimeOnly(9, 0);
                opts.ClosingTime = new TimeOnly(18, 0);
            });
        });
    }

}
