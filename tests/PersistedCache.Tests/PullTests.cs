using PersistedCache.MySql;
using PersistedCache.PostgreSql;
using PersistedCache.Sql;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests;

public abstract class PullTests<TDriver> : BaseTest where TDriver : ISqlCacheDriver
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    private readonly Func<string, IEnumerable<dynamic>> _executeSql;
    
    public PullTests(BaseDatabaseFixture<TDriver> fixture) : base(fixture.PersistedCache)
    {
        _cache = fixture.PersistedCache;
        _executeSql = fixture.ExecuteSql;
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
        Arrange(key, value, Expire.InMilliseconds(1));
        
        // Act
        Thread.Sleep(2);
        var result = _cache.Pull<RandomObject>(key);
        
        // Assert
        result.Should().BeNull();
        var resultAfterPull = _executeSql($"SELECT * FROM {TestConstants.TableName} WHERE `key` = '{key}'");
        resultAfterPull.Should().BeEmpty();
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
public class MySqlPullTestsExecutor : PullTests<MySqlCacheDriver>
{
    public MySqlPullTestsExecutor(MySqlFixture fixture) : base(fixture)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlPullTestsExecutor : PullTests<PostgreSqlCacheDriver>
{
    public PostgreSqlPullTestsExecutor(PostgreSqlFixture fixture) : base(fixture)
    {
    }
}