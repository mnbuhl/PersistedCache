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
            Container = new MsSqlBuilder().Build();
        }
        
        protected override char LeftEscapeCharacter => '[';
        protected override char RightEscapeCharacter => ']';
    }
}