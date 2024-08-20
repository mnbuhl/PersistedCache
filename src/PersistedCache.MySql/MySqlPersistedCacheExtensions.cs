using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersistedCache.Sql;

namespace PersistedCache
{
    public static class MySqlPersistedCacheExtensions
    {
        /// <summary>
        /// Adds a MySql persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the MySql database.</param>
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services,
            string connectionString)
        {
            return services.AddMySqlPersistedCache(new MySqlPersistedCacheOptions(connectionString));
        }

        /// <summary>
        /// Adds a MySql persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the MySql database.</param>
        /// <param name="configure">Action to configure the cache options.</param>
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services,
            string connectionString, Action<MySqlPersistedCacheOptions> configure)
        {
            var options = new MySqlPersistedCacheOptions(connectionString);
            configure(options);

            return services.AddMySqlPersistedCache(options);
        }

        /// <summary>
        /// Adds a MySql persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for the cache.</param>
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services,
            MySqlPersistedCacheOptions options)
        {
            var driver = new MySqlDriver(options);
            var cache = new SqlPersistedCache<MySqlDriver>(driver, options);
            services.TryAddSingleton<IPersistedCache>(cache);
            services.AddSingleton<IPersistedCache<MySqlDriver>>(cache);

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService(_ => new SqlPurgeCacheBackgroundJob<MySqlDriver>(cache, options));
            }

            return services;
        }
    }
}