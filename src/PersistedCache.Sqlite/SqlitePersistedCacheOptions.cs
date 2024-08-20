using PersistedCache.Sql;

namespace PersistedCache;

public class SqlitePersistedCacheOptions : SqlPersistedCacheOptions
{
    public SqlitePersistedCacheOptions(string connectionString) : base(connectionString)
    {
    }
}