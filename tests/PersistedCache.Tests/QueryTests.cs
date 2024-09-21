using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class QueryTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new Fixture();

    public QueryTests(IPersistedCache cache) : base(cache)
    {
        _cache = cache;
    }

    [Theory]
    [InlineData("key*")]
    [InlineData("key?")]
    public void Query_WithValidPattern_ReturnsExpectedValues(string pattern)
    {
        // Arrange
        _cache.Set("key1", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key2", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key3", _fixture.Create<RandomObject>(), Expire.Never);

        // Act
        var result = _cache.Query<RandomObject>(pattern);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void Query_WithNoMatchingPattern_ReturnsEmptyCollection()
    {
        // Arrange
        _cache.Set("key1", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key2", _fixture.Create<RandomObject>(), Expire.Never);

        // Act
        var result = _cache.Query<RandomObject>("keyyyy*");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Query_WithAstrixPattern_ReturnsAllValues()
    {
        // Arrange
        _cache.Set("key1", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key2", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key3", _fixture.Create<RandomObject>(), Expire.Never);

        // Act
        var result = _cache.Query<RandomObject>("*");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Query_WhenExpiredValues_ReturnsOnlyNonExpiredValues()
    {
        // Arrange
        await _cache.SetAsync("key1", _fixture.Create<RandomObject>(), Expire.InMilliseconds(1));
        await _cache.SetAsync("key2", _fixture.Create<RandomObject>(), Expire.Never);
        await _cache.SetAsync("key3", _fixture.Create<RandomObject>(), Expire.InMilliseconds(1));

        // Act
        await Task.Delay(2);
        var result = _cache.QueryAsync<RandomObject>("*");

        // Assert
        (await result).Should().HaveCount(1);
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlQueryTestsExecutor : QueryTests
{
    public MySqlQueryTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlQueryTestsExecutor : QueryTests
{
    public PostgreSqlQueryTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

// [Collection(nameof(SqlServerFixture))]
// public class SqlServerQueryTestsExecutor : QueryTests
// {
//     public SqlServerQueryTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
//     {
//     }
// }

[Collection(nameof(FileSystemFixture))]
public class FileSystemQueryTestsExecutor : QueryTests
{
    public FileSystemQueryTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(SqliteFixture))]
public class SqliteQueryTestsExecutor : QueryTests
{
    public SqliteQueryTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbQueryTestsExecutor : QueryTests
{
    public MongoDbQueryTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache)
    {
    }
}