using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class PullTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new Fixture();
    private readonly Func<string, CacheEntry> _getCacheEntry;
    
    protected PullTests(IPersistedCache cache, Func<string, CacheEntry> getCacheEntry) : base(cache)
    {
        _cache = cache;
        _getCacheEntry = getCacheEntry;
    }
    
    [Fact]
    public void Pull_WithKeyAndValue_ReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        Arrange(key, value);
        
        // Act
        var result = _cache.Pull<RandomObject>(key);
        
        // Assert
        result.Should().BeEquivalentTo(value);
        var resultAfterPull = _cache.Get<RandomObject>(key);
        resultAfterPull.Should().BeNull();
    }
    
    [Fact]
    public void Pull_WithNonExistingKey_ReturnsNull()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        
        // Act
        var result = _cache.Pull<RandomObject>(key);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Pull_WithExpiredKey_ReturnsNullButRemovesKey()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        Arrange(key, value, Expire.InSeconds(1));
        
        // Act
        Thread.Sleep(2000);
        var result = _cache.Pull<RandomObject>(key);
        
        // Assert
        result.Should().BeNull();
        var resultAfterPull = _getCacheEntry(key);
        resultAfterPull.Should().BeNull();
    }
    
    [Fact]
    public async Task PullAsync_WithKeyAndValue_ReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        Arrange(key, value);
        
        // Act
        var result = await _cache.PullAsync<RandomObject>(key);
        
        // Assert
        result.Should().BeEquivalentTo(value);
        var resultAfterPull = _cache.Get<RandomObject>(key);
        resultAfterPull.Should().BeNull();
    }
    
    private void Arrange<T>(string key, T value, Expire? expire = null)
    {
        _cache.Set(key, value, expire ?? Expire.InMinutes(5));
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlPullTestsExecutor : PullTests
{
    public MySqlPullTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlPullTestsExecutor : PullTests
{
    public PostgreSqlPullTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry)
    {
    }
}
    
[Collection(nameof(SqlServerFixture))]
public class SqlServerPullTestsExecutor : PullTests
{
    public SqlServerPullTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry)
    {
    }
}
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemPullTestsExecutor : PullTests
{
    public FileSystemPullTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqlitePullTestsExecutor : PullTests
{
    public SqlitePullTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry)
    {
    }
}