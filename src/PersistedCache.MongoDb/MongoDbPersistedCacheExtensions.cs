using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PersistedCache;

public static class MongoDbPersistedCacheExtensions
{
    /// <summary>
    /// Adds a MongoDB persisted cache to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <param name="databaseName">The MongoDB database name.</param>
    public static IServiceCollection AddMongoDbPersistedCache(this IServiceCollection services, string connectionString, string databaseName)
    {
        return services.AddMongoDbPersistedCache(new MongoDbPersistedCacheOptions(connectionString, databaseName));
    }
    
    /// <summary>
    /// Adds a MongoDB persisted cache to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <param name="databaseName">The MongoDB database name.</param>
    /// <param name="configure">The options configuration action.</param>
    public static IServiceCollection AddMongoDbPersistedCache(this IServiceCollection services, string connectionString, string databaseName, Action<MongoDbPersistedCacheOptions> configure)
    {
        var options = new MongoDbPersistedCacheOptions(connectionString, databaseName);
        configure(options);

        return services.AddMongoDbPersistedCache(options);
    }
    
    /// <summary>
    /// Adds a MongoDB persisted cache to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The MongoDB persisted cache options.</param>
    public static IServiceCollection AddMongoDbPersistedCache(this IServiceCollection services, MongoDbPersistedCacheOptions options)
    {
        var cache = new MongoDbPersistedCache(options);
        services.TryAddSingleton<IPersistedCache>(cache);
        services.AddSingleton<IPersistedCache<MongoDbDriver>>(cache);

        if (options.PurgeExpiredEntries)
        {
            services.AddHostedService(_ => new MongoDbPurgeCacheBackgroundJob(cache, options));
        }

        return services;
    }
}