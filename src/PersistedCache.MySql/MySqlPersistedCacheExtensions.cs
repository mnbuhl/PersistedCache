using Microsoft.Extensions.DependencyInjection;

namespace PersistedCache.MySql
{
    public static class MySqlPersistedCacheExtensions
    {
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services, string connectionString)
        {
            var options = new PersistedCacheOptions(connectionString);
            return services.AddMySqlPersistedCache(options);
        }
        
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services, PersistedCacheOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton<ICacheDriver, MySqlPersistedCacheDriver>();
            services.AddSingleton<IPersistedCache, PersistedCache>();

            return services;
        }
    }
}