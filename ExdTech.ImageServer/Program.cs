using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ExdTech.ImageServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)     // environment variables take precedence over appsettings values.
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
