using PersistedCache.PostgreSql;
using PersistedCache.Tests.Common;
using Testcontainers.PostgreSql;

namespace PersistedCache.Tests.Fixtures;

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
}