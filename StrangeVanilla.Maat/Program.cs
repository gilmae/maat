using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;

namespace StrangeVanilla.Maat
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Maat Connection String: {Environment.GetEnvironmentVariable("MAATCONNSTR")}");
            Console.WriteLine($"Enbilulu host: {Environment.GetEnvironmentVariable("ENBILULUHOST")}");
            Console.WriteLine($"Enbilulu port: {Environment.GetEnvironmentVariable("ENBILULUPORT")}");
            Console.WriteLine($"File Store COnnection String: {Environment.GetEnvironmentVariable("FSSTORECONNSTR")}");
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
                .CreateDefaultBuilder(args)
                .UseUrls("http://0.0.0.0:6767")
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
