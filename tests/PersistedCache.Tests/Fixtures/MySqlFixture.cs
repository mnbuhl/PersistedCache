using System.Collections.Generic;
using Dapper;
using PersistedCache.MySql;
using PersistedCache.Tests.Common;
using Testcontainers.MySql;
using Xunit;


namespace PersistedCache.Tests.Fixtures
{
    [CollectionDefinition(nameof(MySqlFixture))]
    public class MySqlFixture : BaseDatabaseFixture<MySqlCacheDriver>, ICollectionFixture<MySqlFixture>
    {
        public MySqlFixture()
        {
            Container = new MySqlBuilder()
                .WithDatabase("PersistedCache")
                .WithUsername("root")
                .WithPassword("root")
                .Build();
        }
        
        public override IEnumerable<object> GetCacheEntries()
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.Query($"SELECT * FROM `{TestConstants.TableName}`");
            }
        }

        public override object GetCacheEntry(string key)
        {
            using (var connection = Driver.CreateConnection())
            {
                return connection.QueryFirstOrDefault($"SELECT * FROM `{TestConstants.TableName}` WHERE `key` = @Key", new { Key = key });
            }
        }
    }
}