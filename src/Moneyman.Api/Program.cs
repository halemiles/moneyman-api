using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Moneyman.Api
{
    public class Program
    {
        protected Program() { }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Reads the "Sentry" config section (incl. Dsn) and the hosting
                    // environment automatically. No-op when no Dsn is configured.
                    webBuilder.UseSentry();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
