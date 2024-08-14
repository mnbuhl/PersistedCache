using System;
using Microsoft.Extensions.DependencyInjection;
using PersistedCache.Sql;

namespace PersistedCache.MySql
{
    public static class MySqlPersistedCacheExtensions
    {
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services,
            string connectionString)
        {
            var options = new SqlPersistedCacheOptions(connectionString);
            return services.AddMySqlPersistedCache(options);
        }

        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services,
            string connectionString, Action<SqlPersistedCacheOptions> configure)
        {
            var options = new SqlPersistedCacheOptions(connectionString);
            configure(options);

            return services.AddMySqlPersistedCache(options);
        }

        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services,
            SqlPersistedCacheOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton<ISqlCacheDriver, MySqlPersistedSqlCacheDriver>();
            services.AddSingleton<IPersistedCache, SqlPersistedCache>();

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService<SqlPurgeCacheBackgroundJob>();
            }

            return services;
        }
    }
}