using System.Threading.Tasks;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class ExistsTests : BaseTest
{
    private readonly IPersistedCache _cache;

    protected ExistsTests(IPersistedCache cache) : base(cache)
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
        var result = _cache.Exists(key);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Exists_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        const string key = "key";
        
        // Act
        var result = _cache.Exists(key);
        
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
        var result = _cache.Exists(key);

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
        var result = await _cache.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlExistsTestsExecutor : ExistsTests
{
    public MySqlExistsTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlExistsTestsExecutor : ExistsTests
{
    public PostgreSqlExistsTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqlServerFixture))]
public class SqlServerExistsTestsExecutor : ExistsTests
{
    public SqlServerExistsTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemExistsTestsExecutor : ExistsTests
{
    public FileSystemExistsTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqliteExistsTestsExecutor : ExistsTests
{
    public SqliteExistsTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbExistsTestsExecutor : ExistsTests
{
    public MongoDbExistsTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache)
    {
    }
}