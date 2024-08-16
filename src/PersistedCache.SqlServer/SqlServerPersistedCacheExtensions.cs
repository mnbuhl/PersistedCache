using Microsoft.Extensions.DependencyInjection;
using PersistedCache.Sql;

namespace PersistedCache.SqlServer
{
    public static class SqlServerPersistedCacheExtensions
    {
        /// <summary>
        /// Adds a SqlServer persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the SqlServer database.</param>
        public static IServiceCollection AddSqlServerPersistedCache(this IServiceCollection services,
            string connectionString)
        {
            return services.AddSqlServerPersistedCache(new SqlServerPersistedCacheOptions(connectionString));
        }

        /// <summary>
        /// Adds a SqlServer persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the SqlServer database.</param>
        /// <param name="configure">Action to configure the cache options.</param>
        public static IServiceCollection AddSqlServerPersistedCache(this IServiceCollection services,
            string connectionString, Action<SqlServerPersistedCacheOptions> configure)
        {
            var options = new SqlServerPersistedCacheOptions(connectionString);
            configure(options);

            return services.AddSqlServerPersistedCache(options);
        }

        /// <summary>
        /// Adds a SqlServer persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for the cache.</param>
        public static IServiceCollection AddSqlServerPersistedCache(this IServiceCollection services,
            SqlServerPersistedCacheOptions options)
        {
            services.AddSingleton<ISqlPersistedCacheOptions>(options);
            services.AddSingleton<ISqlCacheDriver, SqlServerCacheDriver>();
            services.AddSingleton<IPersistedCache, SqlPersistedCache>();

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService<SqlPurgeCacheBackgroundJob>();
            }

            return services;
        }
    }
}