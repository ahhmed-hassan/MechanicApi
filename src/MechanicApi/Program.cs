using Microsoft.Extensions.DependencyInjection;

namespace src
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
               // .AddPresentation(builder.Configuration)
                .AddInfrastructure(builder.Configuration)
                .AddApplication()
                ; 

            var app = builder.Build();

            //            app.MapGet("/", () => "Hello World!");
            app.MapControllers();

            app.Run();
        }
    }
}
