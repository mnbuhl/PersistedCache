using PersistedCache.Sql;

namespace PersistedCache.MySql;

public class MySqlPersistedCacheOptions : SqlPersistedCacheOptions
{
    public MySqlPersistedCacheOptions(string connectionString) : base(connectionString)
    {
    }
}