
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
using MechanicInfrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using MechanicInfrastructure.Services;




namespace Microsoft.Extensions.DependencyInjection;


public static class DependecyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        #region  Add Database Context
        services.AddSingleton(TimeProvider.System);
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
        services.AddTransient<IIdenttiyService, IdentityService>();

        #endregion

        #region Authentication
        services
            .AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;
            options.SignIn.RequireConfirmedAccount = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        #endregion
        #region Authorization and Policies 
        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(MechanicApplication.Common.Constants.AuhtorizationConstants.SelfScopedWorkedOrderAccess,
            policy => policy.Requirements.Add(new LaborAssigned()))

            .AddPolicy(MechanicApplication.Common.Constants.AuhtorizationConstants.ManagerOnly,
                       policy => policy.RequireRole(nameof(Role.Manager)));
        #endregion

        #region Caching
        services.AddHybridCache(op => op.DefaultEntryOptions = new()
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromMinutes(2)
        }
        );
        #endregion

        //TODO : Token Provieder

        //TODO: WorkOrderPolicy
        services.AddScoped<IWorkOrderPolicy, AvailabilityChecker>();
        //TODO: Notification Service

        //TODO: IWorkOrderNotifier

        //TODO : IInvoicePDFGenerator

        //TODO: OverdueBookingCleanupService
        return services;
    }
}