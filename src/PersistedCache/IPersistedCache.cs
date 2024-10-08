﻿namespace PersistedCache
{
    public interface IPersistedCache<TDriver> : IPersistedCache where TDriver : class, IDriver;
    
    public interface IPersistedCache
    {
        /// <summary>
        /// Set a value in the cache with an expiry time
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expiry">The expiry time of the cached entry</param>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        void Set<T>(string key, T value, Expire expiry);
        
        /// <summary>
        /// Set a value in the cache with an expiry time asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expiry">The expiry time of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        Task SetAsync<T>(string key, T value, Expire expiry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a value from the cache
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <typeparam name="T">The type of the value to get</typeparam>
        /// <returns>The value from the cache</returns>
        T? Get<T>(string key);
        
        /// <summary>
        /// Get a value from the cache asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        /// <returns>The value from the cache</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get a value from the cache or set it if it doesn't exist
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="valueFactory">The factory to create the value if it doesn't exist</param>
        /// <param name="expiry">The expiry time of the cached entry</param>
        /// <typeparam name="T">The type of the value to get/cache</typeparam>
        /// <returns>The value from the cache or the value created by the factory</returns>
        T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry);
        
        /// <summary>
        /// Get a value from the cache or set it if it doesn't exist asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="valueFactory">The factory to create the value if it doesn't exist</param>
        /// <param name="expiry">The expiry time of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The type of the value to get/cache</typeparam>
        /// <returns>The value from the cache or the value created by the factory</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, Expire expiry, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get a value from the cache or set it if it doesn't exist asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="valueFactory">The async factory to create the value if it doesn't exist</param>
        /// <param name="expiry">The expiry time of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The type of the value to get/cache</typeparam>
        /// <returns>The value from the cache or the value created by the factory</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get values from the cache matching a pattern
        /// </summary>
        /// <param name="pattern">The pattern to match keys</param>
        /// <remarks>
        /// Pattern supports wildcards * for multiple characters and ? for a single character.
        /// See driver specific documentation for additional supported wildcards
        /// </remarks>
        /// <typeparam name="T">The type of the value to get</typeparam>
        /// <returns>The values from the cache</returns>
        IEnumerable<T> Query<T>(string pattern);
        
        /// <summary>
        /// Get values from the cache matching a pattern asynchronously
        /// </summary>
        /// <param name="pattern">The pattern to match keys</param>
        /// <remarks>
        /// Pattern supports wildcards * for multiple characters and ? for a single character.
        /// See driver specific documentation for additional supported wildcards
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The type of the value to get</typeparam>
        /// <returns>The values from the cache</returns>
        Task<IEnumerable<T>> QueryAsync<T>(string pattern, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if the cache has a value
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <returns>True if the value exists, false otherwise</returns>
        bool Has(string key);
        
        /// <summary>
        /// Check if the cache has a value asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if the value exists, false otherwise</returns>
        Task<bool> HasAsync(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Forget a value from the cache
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        void Forget(string key);
        
        /// <summary>
        /// Forget a value from the cache asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task ForgetAsync(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a value from the cache and removes it
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <typeparam name="T">The type of the value to get</typeparam>
        /// <returns>The value from the cache</returns>
        T? Pull<T>(string key);
        
        /// <summary>
        /// Gets a value from the cache and removes it asynchronously
        /// </summary>
        /// <param name="key">The key of the cached entry</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        /// <returns>The value from the cache</returns>
        Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Flush all values from the cache
        /// </summary>
        void Flush();
        
        /// <summary>
        /// Flush all values from the cache asynchronously
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        Task FlushAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Flush values from the cache by pattern
        /// </summary>
        /// <param name="pattern">The pattern to match keys</param>
        /// <remarks>
        /// Pattern supports wildcards * for multiple characters and ? for a single character.
        /// See driver specific documentation for additional supported wildcards
        /// </remarks>
        /// <example>Flush("?sers.*)</example>
        void Flush(string pattern);
        
        /// <summary>
        /// Flush values from the cache by pattern asynchronously
        /// </summary>
        /// <param name="pattern">The pattern to match keys</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <remarks>
        /// Pattern supports wildcards * for multiple characters and ? for a single character.
        /// See driver specific documentation for additional supported wildcards
        /// </remarks>
        /// <example>await FlushAsync("?sers.*")</example>
        Task FlushAsync(string pattern, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Purge the cache of expired entries
        /// </summary>
        void Purge();
        
        /// <summary>
        /// Purge the cache of expired entries asynchronously
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        Task PurgeAsync(CancellationToken cancellationToken = default);
    }
}