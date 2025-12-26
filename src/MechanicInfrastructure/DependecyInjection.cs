
using MechanicDomain.Identity;
using MechanicInfrastructure.Data.Interceptors;
using MechanicInfrastructure.Data.Migrations;
using MechanicInfrastructure.Identity.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using Microsoft.EntityFrameworkCore;
using MechanicApplication.Common.Interfaces;
using MechanicInfrastructure.Data;




namespace Microsoft.Extensions.DependencyInjection;


public static class DependecyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        #region  Add Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrEmpty(connectionString, "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>(); 
        #endregion
        // Add infrastructure services here (e.g., database context, repositories, etc.)
        #region Identity 

        #endregion
        #region Authentication and Authorization
        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(MechanicApplication.Common.Constants.AuhtorizationConstants.SelfScopedWorkedOrderAccess,
            policy => policy.Requirements.Add(new LaborAssigned()))

            .AddPolicy(MechanicApplication.Common.Constants.AuhtorizationConstants.ManagerOnly,
                       policy => policy.RequireRole(nameof(Role.Manager)));
        #endregion

        #region Caching

        #endregion

        //TODO : Token Provieder

        //TODO: WorkOrderPolicy

        //TODO: Notification Service

        //TODO: IWorkOrderNotifier

        //TODO : IInvoicePDFGenerator

        //TODO: OverdueBookingCleanupService
        return services;
    }
}