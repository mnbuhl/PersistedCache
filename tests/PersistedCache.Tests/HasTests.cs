using System.Threading.Tasks;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class HasTests : BaseTest
{
    private readonly IPersistedCache _cache;

    protected HasTests(IPersistedCache cache) : base(cache)
    {
        _cache = cache;
    }
    
    [Fact]
    public void Exists_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        const string key = "key";
        _cache.Set(key, new RandomObject(), Expire.Never);
        
        // Act
        var result = _cache.Has(key);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Exists_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        const string key = "key";
        
        // Act
        var result = _cache.Has(key);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Exists_WithExistingKeyAndExpired_ReturnsFalse()
    {
        // Arrange
        const string key = "key";
        _cache.Set(key, new RandomObject(), Expire.InMilliseconds(1));

        // Act
        System.Threading.Thread.Sleep(2);
        var result = _cache.Has(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        const string key = "key";
        await _cache.SetAsync(key, new RandomObject(), Expire.Never);

        // Act
        var result = await _cache.HasAsync(key);

        // Assert
        result.Should().BeTrue();
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlHasTestsExecutor : HasTests
{
    public MySqlHasTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlHasTestsExecutor : HasTests
{
    public PostgreSqlHasTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
// [Collection(nameof(SqlServerFixture))]
// public class SqlServerHasTestsExecutor : HasTests
// {
//     public SqlServerHasTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
//     {
//     }
// }
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemHasTestsExecutor : HasTests
{
    public FileSystemHasTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqliteHasTestsExecutor : HasTests
{
    public SqliteHasTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbHasTestsExecutor : HasTests
{
    public MongoDbHasTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache)
    {
    }
}