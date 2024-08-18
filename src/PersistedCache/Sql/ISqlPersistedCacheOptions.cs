using System.Text.Json;

namespace PersistedCache.Sql;

internal interface ISqlPersistedCacheOptions
{
    /// <summary>
    /// The name of the table to use for the cache.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the table name is null or empty.</exception>
    string TableName { get; set; }

    /// <summary>
    /// Whether to create the table if it does not exist.
    /// </summary>
    bool CreateTableIfNotExists { get; set; }

    /// <summary>
    /// The connection string to use for the cache.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Whether to purge expired entries.
    /// </summary>
    bool PurgeExpiredEntries { get; set; }

    /// <summary>
    /// The interval at which to purge expired entries.
    /// </summary>
    TimeSpan PurgeInterval { get; set; }

    /// <summary>
    /// The options to use for JSON serialization.
    /// </summary>
    JsonSerializerOptions JsonOptions { get; set; }
}