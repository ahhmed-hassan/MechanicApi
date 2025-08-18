using Asp.Versioning;
using MechanicApi.Infrastructure;
using MechanicApi.Services;
using MechanicApplication.Common.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAppOutpurCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.SizeLimit = 1024 * 1024 * 100; // 100 MB
            options.AddBasePolicy(policy =>
                policy.Expire(TimeSpan.FromMinutes(1))
           );
        });
        return services;
    }

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter("SlidingWindow", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(10);
                limiterOptions.PermitLimit = 100;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 9;
                limiterOptions.AutoReplenishment = true;
                limiterOptions.SegmentsPerWindow = 6;
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
        return services;
    }

    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("api-version"),
                new QueryStringApiVersionReader("api-version")
            );
        }).AddMvc().AddApiExplorer();

        return services;
    }
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        throw new NotImplementedException("AddApiDocumentation is not implemented yet.");
        return services;
    }

    public static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options => options
            .JsonSerializerOptions
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        return services;
    }

    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }
    //public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    //{
    //    var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()!;

    //    services.AddCors(options => options.AddPolicy(
    //        appSettings.CorsPolicyName,
    //        policy => policy
    //            .WithOrigins(appSettings.AllowedOrigins!)
    //            .AllowAnyHeader()
    //            .AllowAnyMethod()
    //            .AllowCredentials()));

    //    return services;
    //}

    public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
    {
        // 1. Exception handling should be FIRST to catch all errors
        app.UseExceptionHandler();

        // 2. Status code pages for handling HTTP status codes
        app.UseStatusCodePages();

        // 3. HTTPS redirection (before any other middleware that might generate URLs)
        app.UseHttpsRedirection();

        // 4. Serilog request logging (early to log all requests)
        app.UseSerilogRequestLogging();

        // 5. CORS (before authentication/authorization)
        app.UseCors(configuration["AppSettings:CorsPolicyName"]!);

        // 6. Rate limiting (before authentication to protect auth endpoints)
        app.UseRateLimiter();

        // 7. Authentication (must come before authorization)
        app.UseAuthentication();

        // 8. Authorization (must come after authentication)
        app.UseAuthorization();

        // 9. Output caching (after auth to cache based on user context)
        app.UseOutputCache();

        return app;
    }

}

