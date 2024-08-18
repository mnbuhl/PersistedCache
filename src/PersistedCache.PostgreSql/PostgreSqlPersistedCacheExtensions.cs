using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersistedCache.Sql;

namespace PersistedCache.PostgreSql
{
    public static class PostgreSqlPersistedCacheExtensions
    {
        /// <summary>
        /// Adds a PostgreSQL persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the PostgreSQL database.</param>
        public static IServiceCollection AddPostgreSqlPersistedCache(this IServiceCollection services,
            string connectionString)
        {
            return services.AddPostgreSqlPersistedCache(new PostgreSqlPersistedCacheOptions(connectionString));
        }

        /// <summary>
        /// Adds a PostgreSQL persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the PostgreSQL database.</param>
        /// <param name="configure">Action to configure the cache options.</param>
        public static IServiceCollection AddPostgreSqlPersistedCache(this IServiceCollection services,
            string connectionString, Action<PostgreSqlPersistedCacheOptions> configure)
        {
            var options = new PostgreSqlPersistedCacheOptions(connectionString);
            configure(options);

            return services.AddPostgreSqlPersistedCache(options);
        }

        /// <summary>
        /// Adds a PostgreSQL persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for the cache.</param>
        public static IServiceCollection AddPostgreSqlPersistedCache(this IServiceCollection services,
            PostgreSqlPersistedCacheOptions options)
        {
            var driver = new PostgreSqlDriver(options);
            var cache = new SqlPersistedCache<PostgreSqlDriver>(driver, options);
            services.TryAddSingleton<IPersistedCache>(cache);
            services.AddSingleton<IPersistedCache<PostgreSqlDriver>>(cache);

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService(_ => new SqlPurgeCacheBackgroundJob<PostgreSqlDriver>(cache, options));
            }

            return services;
        }
    }
}