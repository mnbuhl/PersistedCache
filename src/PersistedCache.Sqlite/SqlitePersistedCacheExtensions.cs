using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersistedCache.Sql;

namespace PersistedCache.Sqlite;

public static class SqlitePersistedCacheExtensions
{
        /// <summary>
        /// Adds a Sqlite persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the Sqlite database.</param>
        public static IServiceCollection AddSqlitePersistedCache(this IServiceCollection services,
            string connectionString)
        {
            return services.AddSqlitePersistedCache(new SqlitePersistedCacheOptions(connectionString));
        }

        /// <summary>
        /// Adds a Sqlite persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the Sqlite database.</param>
        /// <param name="configure">Action to configure the cache options.</param>
        public static IServiceCollection AddSqlitePersistedCache(this IServiceCollection services,
            string connectionString, Action<SqlitePersistedCacheOptions> configure)
        {
            var options = new SqlitePersistedCacheOptions(connectionString);
            configure(options);

            return services.AddSqlitePersistedCache(options);
        }

        /// <summary>
        /// Adds a Sqlite persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for the cache.</param>
        public static IServiceCollection AddSqlitePersistedCache(this IServiceCollection services,
            SqlitePersistedCacheOptions options)
        {
            var driver = new SqliteDriver(options);
            var cache = new SqlPersistedCache<SqliteDriver>(driver, options);
            services.TryAddSingleton<IPersistedCache>(cache);
            services.AddSingleton<IPersistedCache<SqliteDriver>>(cache);

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService(_ => new SqlPurgeCacheBackgroundJob<SqliteDriver>(cache, options));
            }

            return services;
        }
}