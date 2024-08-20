using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersistedCache.Sql;

namespace PersistedCache
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
            var driver = new SqlServerDriver(options);
            var cache = new SqlPersistedCache<SqlServerDriver>(driver, options);
            services.TryAddSingleton<IPersistedCache>(cache);
            services.AddSingleton<IPersistedCache<SqlServerDriver>>(cache);

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService(_ => new SqlPurgeCacheBackgroundJob<SqlServerDriver>(cache, options));
            }

            return services;
        }
    }
}