using System.Collections.Generic;
using Dapper;
using PersistedCache.SqlServer;
using PersistedCache.Tests.Common;
using Testcontainers.MsSql;
using Xunit;

namespace PersistedCache.Tests.Fixtures
{
    [CollectionDefinition(nameof(SqlServerFixture))]
    public class SqlServerFixture : BaseDatabaseFixture<SqlServerCacheDriver>, ICollectionFixture<SqlServerFixture>
    {
        public SqlServerFixture()
        {
            Container = new MsSqlBuilder()
                .WithPassword("Password123!")
                .Build();
        }
        
        public override IEnumerable<object> GetCacheEntries()
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.Query($"SELECT * FROM [{TestConstants.TableName}]");
            }
        }

        public override object GetCacheEntry(string key)
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.QueryFirstOrDefault($"SELECT * FROM [{TestConstants.TableName}] WHERE [key] = @Key",
                    new { Key = key });
            }
        }
    }
}