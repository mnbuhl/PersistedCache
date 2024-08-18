namespace PersistedCache.Sql;

public abstract class SqlPersistedCacheOptions : PersistedCacheOptions, ISqlPersistedCacheOptions
{
    private string _tableName = "persisted_cache";

    protected SqlPersistedCacheOptions(string connectionString)
    {
        ConnectionString = !string.IsNullOrWhiteSpace(connectionString) 
            ? connectionString 
            : throw new ArgumentException("Connection string cannot be null or empty.");
    }
        
    /// <summary>
    /// The name of the table to use for the cache.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the table name is null or empty.</exception>
    public string TableName
    {
        get => _tableName; 
        set => _tableName = !string.IsNullOrWhiteSpace(value) 
            ? value 
            : throw new ArgumentException("Table name cannot be null or empty.");
    }
        
    /// <summary>
    /// Whether to create the table if it does not exist.
    /// </summary>
    public bool CreateTableIfNotExists { get; set; } = true;
        
    /// <summary>
    /// The connection string to use for the cache.
    /// </summary>
    public string ConnectionString { get; }
}