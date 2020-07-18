using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using SimpleRepo;

namespace SV.Maat.lib
{
    public class DbContextBuilder
    {
        IDbContext _context;
        IConfiguration _config;
        public DbContextBuilder(IDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public DbContextBuilder UseConnectionString(string connectionString)
        {
            _context.SetConnectionString(connectionString);
            return this;
        }


        public DbContextBuilder UseConnectionStringName(string connectionStringName)
        {
            return UseConnectionString(_config.GetConnectionString(connectionStringName));
        }
    }

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDbContext(this IApplicationBuilder builder, Action<DbContextBuilder> configure)
        {

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            DbContextBuilder dbContextBuilder = builder.ApplicationServices.GetService(typeof(DbContextBuilder)) as DbContextBuilder;

            if (dbContextBuilder == null)
            {
                throw new ArgumentNullException(nameof(dbContextBuilder));
            }

            configure(dbContextBuilder);

            return builder;

        }
    }
}
