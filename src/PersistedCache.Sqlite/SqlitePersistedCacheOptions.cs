using PersistedCache.Sql;

namespace PersistedCache.Sqlite;

public class SqlitePersistedCacheOptions : SqlPersistedCacheOptions
{
    public SqlitePersistedCacheOptions(string connectionString) : base(connectionString)
    {
    }
}