using System;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<ICacheDriver, MySqlPersistedCacheDriver>();
            services.AddSingleton<IPersistedCache, SqlPersistedCache>();

            return services;
        }
    }
}