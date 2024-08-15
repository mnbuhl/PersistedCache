using PersistedCache.Sql;

namespace PersistedCache.PostgreSql;

public class PostgreSqlPersistedCacheOptions : SqlPersistedCacheOptions
{
    public PostgreSqlPersistedCacheOptions(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// The search path/schema to use for the cache (default: public).
    /// </summary>
    public string Schema { get; set; } = "public";
}