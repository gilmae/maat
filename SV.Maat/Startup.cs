using Events;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StrangeVanilla.Blogging.Events;
using SV.Maat.IndieAuth;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.lib.FileStore;
using SV.Maat.Syndications;
using SV.Maat.Syndications.Models;
using SV.Maat.Users;
using SV.Maat.Projections;
using SV.Maat.lib.Pipelines;
using SV.Maat.Reactors;
using SV.Maat.ExternalNetworks;
using SV.Maat.lib;
using SV.Maat.Mastodon;

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
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Shared/Views/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme
            ).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.LoginPath = "/User/signin";
                    options.LogoutPath = "/User/signout";
                });

            services.AddAuthentication(opt => {
                opt.DefaultScheme = IndieAuthTokenHandler.SchemeName;
            }).AddScheme<IndieAuthOptions, IndieAuthTokenHandler>(IndieAuthTokenHandler.SchemeName, op => { });


            services.AddMvc().AddNewtonsoftJson();
            services.AddRazorPages()
                .AddNewtonsoftJson();
            services.AddControllers()
                .AddNewtonsoftJson();

            services.Configure<CertificateStorage>(Configuration.GetSection("CertificateStorage"));

            services.AddSingleton<IEventStore<Entry>, PgStore<Entry>>();
            services.AddSingleton<IEventStore<Media>, PgStore<Media>>();
            services.AddSingleton<IEventStore, PgStoreBase>();

            services.AddSingleton<IEntryProjection, EntryProjection>();
            services.AddSingleton<IRepliesProjection, RepliesProjection>();
            services.AddSingleton<IProjection<Media>, MemoryProjection<Media>>();

            services.AddSingleton<IFileStore, FSStore>();
            services.AddTransient<IUserStore, UserStore>();
            services.AddTransient<ISyndicationStore, SyndicationStore>();
            services.AddTransient<IAuthenticationRequestStore, AuthenticationRequestStore>();
            services.AddTransient<IAccessTokenStore, AccessTokenStore>();
            
            services.AddSingleton(typeof(TokenSigning));

            services.AddTransient(typeof(CommandHandler));

            services.AddTwitter();
            services.AddTransient<ISyndicationNetwork, Mastodon.Mastodon>();
            services.AddTransient<ISyndicationNetwork, Pinboard>();

            services.AddPipelines();
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

            app.UseRequestLogger();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            app.UsePipelines(builder =>
            {
                builder.UseSyndicateEntry();
            });
        }
    }
}
