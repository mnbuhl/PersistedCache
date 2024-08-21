namespace PersistedCache;

public class MongoDbPersistedCacheOptions : PersistedCacheOptions
{
    /// <summary>
    /// Initializes a new instance of <see cref="MongoDbPersistedCacheOptions"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to use for the cache.</param>
    /// <param name="databaseName">The name of the database to use for the cache.</param>
    public MongoDbPersistedCacheOptions(string connectionString, string databaseName)
    {
        ConnectionString = connectionString;
        DatabaseName = databaseName;
    }
    
    /// <summary>
    /// The connection string to use for the cache.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// The name of the database to use for the cache.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// The connection string to use for the cache.
    /// </summary>
    public string CollectionName { get; set; } = "persistedCache";
}