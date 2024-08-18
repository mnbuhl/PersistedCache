using System.Collections.Generic;
using Dapper;
using PersistedCache.PostgreSql;
using PersistedCache.Tests.Common;
using Testcontainers.PostgreSql;
using Xunit;

namespace PersistedCache.Tests.Fixtures
{
    [CollectionDefinition(nameof(PostgreSqlFixture))]
    public class PostgreSqlFixture : BaseDatabaseFixture<PostgreSqlCacheDriver>, ICollectionFixture<PostgreSqlFixture>
    {
        public PostgreSqlFixture()
        {
            Container = new PostgreSqlBuilder()
                .WithDatabase("PersistedCache")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }
        
        public override IEnumerable<object> GetCacheEntries()
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.Query($"SELECT * FROM {TestConstants.TableName}");
            }
        }

        public override object GetCacheEntry(string key)
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.QueryFirstOrDefault($@"SELECT * FROM ""{TestConstants.TableName}"" WHERE ""key"" = @Key", new { Key = key });
            }
        }
    }
}