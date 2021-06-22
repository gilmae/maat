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
using SV.Maat.Projections;
using SV.Maat.lib.Pipelines;
using SV.Maat.Reactors;
using SV.Maat.Webmention;
using SV.Maat.ExternalNetworks;
using SV.Maat.lib;
using SV.Maat.Mastodon;
using SimpleDbContext;
using SimpleDbContext.Npgsql;
using SimpleDbContext.AspNetCore;
using Users;
using SV.Maat.Microblog;
using OpenTelemetry.Trace;
using System;
using OpenTelemetry.Resources;

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
            services.AddMemoryCache();
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Shared/Views/{0}" + RazorViewEngine.ViewExtension);
            });

            //services.AddHoneycomb(Configuration);
            var oltpOptions = Configuration.GetSection("HoneycombSettings");
            services.AddOpenTelemetryTracing(ot =>
                ot.AddSource("Maat")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("maat-oltp"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(o => {
                    o.Endpoint = new Uri("https://api.honeycomb.io:443");
                    o.Headers = $"x-honeycomb-team={oltpOptions.GetValue<string>("TeamId")}, x-honeycomb-dataset={oltpOptions.GetValue<string>("DefaultDataSet")}";
                })
            );
            

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

            services.AddHealthChecks();
            services.AddMvc().AddNewtonsoftJson();
            services.AddRazorPages().AddNewtonsoftJson();
            services.AddControllers().AddNewtonsoftJson();

            services.Configure<CertificateStorage>(Configuration.GetSection("CertificateStorage"));

            services.AddSingleton<IDbContext, PgContext>();
            services.AddTransient(typeof(DbContextBuilder));

            services.AddSingleton<IEventStore<Entry>, PgStore<Entry>>();
            services.AddSingleton<IEventStore<Media>, PgStore<Media>>();
            services.AddSingleton<IEventStore, PgStoreBase>();

            services.AddSingleton<IEntryProjection, EntryProjection>();
            services.AddSingleton<IRepliesProjection, RepliesProjection>();
            services.AddSingleton<ISyndicationsProjection, SyndicationsProjection>();
            services.AddSingleton<IProjection<Media>, MemoryProjection<Media>>();

            services.AddSingleton<IFileStore, FSStore>();
            services.AddTransient<IUserStore, UserStore>();
            services.AddTransient<ISyndicationStore, SyndicationStore>();
            services.AddTransient<IAuthenticationRequestStore, AuthenticationRequestStore>();
            services.AddTransient<IAccessTokenStore, AccessTokenStore>();
            
            services.AddSingleton(typeof(TokenSigning));

            services.AddTransient(typeof(CommandHandler));

            services.AddTwitter();
            services.AddMastodon();
            services.AddMicroblog();
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
            
            app.UseStaticFiles();

            //app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthorization();

            //app.UseHoneycomb();
            app.UseRequestLogger();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health");
            });

            app.UsePipelines(builder =>
            {
                builder.UseSyndicateEntry();
                builder.UseBookmarkArchival();
                builder.UseWebmentionSender();
            });

            app.UseDbContext(builder =>
            {
                builder.UseConnectionStringName("maat");
            });
        }
    }
}
