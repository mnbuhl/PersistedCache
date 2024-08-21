using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PersistedCache;

public static class FileSystemPersistedCacheExtensions
{
    /// <summary>
        /// Adds a FileSystem persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="path">Path to the cache folder.</param>
        public static IServiceCollection AddFileSystemPersistedCache(this IServiceCollection services,
            string path)
        {
            return services.AddFileSystemPersistedCache(new FileSystemPersistedCacheOptions(path));
        }

        /// <summary>
        /// Adds a FileSystem persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="path">Path to the cache folder.</param>
        /// <param name="configure">Action to configure the cache options.</param>
        public static IServiceCollection AddFileSystemPersistedCache(this IServiceCollection services,
            string path, Action<FileSystemPersistedCacheOptions> configure)
        {
            var options = new FileSystemPersistedCacheOptions(path);
            configure(options);

            return services.AddFileSystemPersistedCache(options);
        }

        /// <summary>
        /// Adds a FileSystem persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for the cache.</param>
        public static IServiceCollection AddFileSystemPersistedCache(this IServiceCollection services,
            FileSystemPersistedCacheOptions options)
        {
            var cache = new FileSystemPersistedCache(options);
            
            services.TryAddSingleton<IPersistedCache>(cache);
            services.AddSingleton<IPersistedCache<FileSystemDriver>>(cache);

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService(_ => new FileSystemPurgeCacheBackgroundJob(cache, options));
            }

            return services;
        }
}