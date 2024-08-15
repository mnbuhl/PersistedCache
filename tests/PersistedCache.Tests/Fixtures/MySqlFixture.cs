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

        protected override char LeftEscapeCharacter => '`';
        protected override char RightEscapeCharacter => '`';
    }
}