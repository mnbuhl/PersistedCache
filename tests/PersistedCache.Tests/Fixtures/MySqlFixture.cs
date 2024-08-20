using System.Collections.Generic;
using Dapper;
using PersistedCache.Tests.Common;
using Testcontainers.MySql;
using Xunit;


namespace PersistedCache.Tests.Fixtures;

[CollectionDefinition(nameof(MySqlFixture))]
public class MySqlFixture : BaseDatabaseFixture<MySqlDriver>, ICollectionFixture<MySqlFixture>
{
    public MySqlFixture()
    {
        Container = new MySqlBuilder()
            .WithDatabase("PersistedCache")
            .WithUsername("root")
            .WithPassword("root")
            .Build();
    }
        
    public override IEnumerable<CacheEntry> GetCacheEntries()
    {
        using var connection = Driver.CreateConnection();
        return connection.Query<CacheEntry>($"SELECT * FROM `{TestConstants.TableName}`");
    }

    public override CacheEntry GetCacheEntry(string key)
    {
        using var connection = Driver.CreateConnection();
        return connection.QueryFirstOrDefault<CacheEntry>($"SELECT * FROM `{TestConstants.TableName}` WHERE `key` = @Key", new { Key = key });
    }
}