using DotNet.Testcontainers.Containers;
using PersistedCache.MySql;
using Testcontainers.MySql;


namespace PersistedCache.Tests.Fixtures;

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
}