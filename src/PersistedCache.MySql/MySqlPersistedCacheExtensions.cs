using Microsoft.Extensions.DependencyInjection;

namespace PersistedCache.MySql
{
    public static class MySqlPersistedCacheExtensions
    {
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services, string connectionString)
        {
            var options = new MySqlCacheOptions(connectionString)
            {
                TableName = "test"
            };
            return services.AddMySqlPersistedCache(options);
        }
        
        public static IServiceCollection AddMySqlPersistedCache(this IServiceCollection services, MySqlCacheOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton<ICacheDriver, MySqlCacheDriver>();
            services.AddSingleton<IPersistedCache, PersistedCache>();

            return services;
        }
    }
}