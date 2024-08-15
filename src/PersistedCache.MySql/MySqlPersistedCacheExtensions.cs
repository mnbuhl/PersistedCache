using Microsoft.Extensions.DependencyInjection;
using PersistedCache.Sql;

namespace PersistedCache.MySql
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
            services.AddSingleton<ISqlPersistedCacheOptions>(options);
            services.AddSingleton<ISqlCacheDriver, MySqlCacheDriver>();
            services.AddSingleton<IPersistedCache, SqlPersistedCache>();

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService<SqlPurgeCacheBackgroundJob>();
            }

            return services;
        }
    }
}