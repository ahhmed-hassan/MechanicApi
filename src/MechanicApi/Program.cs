using MechanicInfrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace src
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddPresentation(builder.Configuration)
                .AddInfrastructure(builder.Configuration)
                .AddApplication()
                ; 

            var app = builder.Build();

            //            app.MapGet("/", () => "Hello World!");
//            app.MapControllers();
            if(app.Environment.IsDevelopment())
            {
                await app.InitialiseDatabaseAsync(); 
            }
            app.Run();
        }
    }
}
