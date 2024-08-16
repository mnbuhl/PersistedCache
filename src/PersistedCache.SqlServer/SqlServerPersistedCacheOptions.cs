using PersistedCache.Sql;

namespace PersistedCache.SqlServer;

public class SqlServerPersistedCacheOptions : SqlPersistedCacheOptions
{
    /// <summary>
    /// The search path/schema to use for the cache (default: public).
    /// </summary>
    public string Schema { get; set; } = "dbo";
    
    public SqlServerPersistedCacheOptions(string connectionString) : base(connectionString)
    {
    }
}