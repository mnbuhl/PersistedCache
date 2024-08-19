using System;
using System.Collections.Generic;
using Dapper;
using PersistedCache.Sqlite;
using PersistedCache.Tests.Common;
using Xunit;

namespace PersistedCache.Tests.Fixtures
{
    [CollectionDefinition(nameof(SqliteFixture))]
    public class SqliteFixture : BaseDatabaseFixture<SqliteDriver>, ICollectionFixture<SqliteFixture>
    {
        public SqliteFixture()
        {
            ConnectionString = $"Data Source={Guid.NewGuid()}.db;Mode=Memory;Cache=Shared";
        }
        
        public override IEnumerable<CacheEntry> GetCacheEntries()
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.Query<CacheEntry>($@"SELECT * FROM ""{TestConstants.TableName}""");
            }
        }

        public override CacheEntry GetCacheEntry(string key)
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.QueryFirstOrDefault<CacheEntry>($@"SELECT * FROM ""{TestConstants.TableName}"" WHERE ""key"" = @Key", new { Key = key });
            }
        }
    }
}