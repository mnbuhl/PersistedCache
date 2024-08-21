using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class GetOrSetTests : BaseTest
{
    private readonly IPersistedCache _cache;    
    private readonly Fixture _fixture = new Fixture();
    
    protected GetOrSetTests(IPersistedCache cache) : base(cache)
    {
        _cache = cache;
    }

    [Fact]
    public void GetOrSet_WithKeyAndValue_ReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = _cache.GetOrSet(key, () => value, Expire.InMinutes(5));

        // Assert
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void GetOrSet_WithKeyWhenValueExists_ReturnsExistingValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var oldValue = _fixture.Create<RandomObject>();
        Arrange(key, oldValue);
        
        var newValue = _fixture.Create<RandomObject>();;

        // Act
        var result = _cache.GetOrSet(key, () => newValue, Expire.InMinutes(5));

        // Assert
        result.Should().BeEquivalentTo(oldValue);
    }

    [Fact]
    public void GetOrSet_WhenKeyDoesNotExist_SetsAndReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = _cache.GetOrSet(key, () => value, Expire.InMinutes(5));
        
        // Assert
        result.Should().Be(value);
        
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task GetOrSetAsync_WithKeyAndValue_ReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = await _cache.GetOrSetAsync(key, () => Task.FromResult(value), Expire.InMinutes(5));

        // Assert
        result.Should().BeEquivalentTo(value);
    }

    private void Arrange<T>(string key, T value, Expire? expire = null)
    {
        _cache.Set(key, value, expire ?? Expire.InMinutes(5));
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlGetOrSetTestsExecutor : GetOrSetTests
{
    public MySqlGetOrSetTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlGetOrSetTestsExecutor : GetOrSetTests
{
    public PostgreSqlGetOrSetTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqlServerFixture))]
public class SqlServerGetOrSetTestsExecutor : GetOrSetTests
{
    public SqlServerGetOrSetTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemGetOrSetTestsExecutor : GetOrSetTests
{
    public FileSystemGetOrSetTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqliteGetOrSetTestsExecutor : GetOrSetTests
{
    public SqliteGetOrSetTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbGetOrSetTestsExecutor : GetOrSetTests
{
    public MongoDbGetOrSetTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache)
    {
    }
}