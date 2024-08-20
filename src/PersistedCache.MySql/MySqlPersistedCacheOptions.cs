using PersistedCache.Sql;

namespace PersistedCache;

public class MySqlPersistedCacheOptions : SqlPersistedCacheOptions
{
    public MySqlPersistedCacheOptions(string connectionString) : base(connectionString)
    {
    }
}