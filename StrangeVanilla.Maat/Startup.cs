using Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nancy.Owin;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.lib.MessageBus;

namespace StrangeVanilla.Maat
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                ;

            Configuration = builder.Build();


        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventStore<Entry>, PgStore<Entry>>();
            services.AddSingleton<IEventStore<Media>, PgStore<Media>>();
            services.AddSingleton<IMessageBus<Entry>, EnbiluluBus<Entry>>();

            services.AddSingleton<IProjection<Entry>, Projection<Entry>>();

            //// Configure using a sub-section of the environment variables.
            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            //services.Configure<ConnectionStringSettingsCollection>(Configuration.GetSection("ConnectionStrings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //allow us to serve static files from the wwwroot folder
            app.UseStaticFiles();

            //add Nancy with custom Bootstrapper
            app.UseOwin(x => x.UseNancy(new NancyOptions
            {
                Bootstrapper = new Bootstrapper(app.ApplicationServices)
            }));


        }
    }
}
