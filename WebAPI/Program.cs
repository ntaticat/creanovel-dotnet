using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
                {
                    var db = scope.ServiceProvider.GetRequiredService<CreanovelDbContext>();
                    db.Database.Migrate();
                }

            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
