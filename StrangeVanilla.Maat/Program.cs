using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace StrangeVanilla.Maat
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
                .CreateDefaultBuilder(args)
                .ConfigureKestrel((ct, k) => k.AllowSynchronousIO = true)
                .ConfigureLogging((host, logging) =>
                {
                    var env = host.HostingEnvironment;

                    if (env.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })
                .UseStartup<Startup>();
    }
}
