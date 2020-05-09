using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using SV.Maat.IndieAuth;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.FileStore;
using SV.Maat.lib.MessageBus;
using SV.Maat.lib.Repository;
using SV.Maat.Syndications;
using SV.Maat.Syndications.Models;
using SV.Maat.Users;

namespace SV.Maat
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();
            services.AddRazorPages()
                .AddNewtonsoftJson();
            services.AddControllers()
                .AddNewtonsoftJson();

            var syndicationNetworksSection =
                Configuration.GetSection("SyndicationNetworks");
            services.Configure<SyndicationNetworks>(syndicationNetworksSection);

            services.AddSingleton<IEventStore<Entry>, PgStore<Entry>>();
            services.AddSingleton<IEventStore<Media>, PgStore<Media>>();
            services.AddSingleton<IMessageBus<Entry>, EnbiluluBus<Entry>>();

            services.AddSingleton<IProjection<Entry>, MemoryProjection<Entry>>();
            services.AddSingleton<IProjection<Media>, MemoryProjection<Media>>();

            services.AddSingleton<IFileStore, FSStore>();
            services.AddTransient<IUserStore, UserStore>();
            services.AddTransient<ISyndicationStore, SyndicationStore>();
            services.AddTransient<IAuthenticationRequestStore, AuthenticationRequestStore>();
            services.AddTransient<IRepository<AccessToken>, AccessTokenStore>();


            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Shared/Views/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddAuthentication(opt => {
                opt.DefaultScheme = IndieAuthTokenHandler.SchemeName;
            })
                .AddScheme<IndieAuthOptions, IndieAuthTokenHandler>(IndieAuthTokenHandler.SchemeName, op => { });
        }

       // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
