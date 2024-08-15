using PersistedCache.MySql;
using PersistedCache.PostgreSql;
using PersistedCache.Sql;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests;

public abstract class ForgetTests<TDriver> : BaseTest where TDriver : ISqlCacheDriver
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    
    public ForgetTests(BaseDatabaseFixture<TDriver> fixture) : base(fixture.PersistedCache)
    {
        _cache = fixture.PersistedCache;
    }
    
    [Fact]
    public void Forget_WithKey_RemovesValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        Arrange(key, value);
        
        // Act
        _cache.Forget(key);
        
        // Assert
        var result = _cache.Get<RandomObject>(key);
        result.Should().BeNull();
    }
    
    [Fact]
    public void Forget_WithNonExistingKey_DoesNotThrow()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        
        // Act
        Action act = () => _cache.Forget(key);
        
        // Assert
        act.Should().NotThrow();
    }
    
    [Fact]
    public async Task ForgetAsync_WithKey_RemovesValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        Arrange(key, value);
        
        // Act
        await _cache.ForgetAsync(key);
        
        // Assert
        var result = _cache.Get<RandomObject>(key);
        result.Should().BeNull();
    }
    
    private void Arrange<T>(string key, T value, Expire? expire = null)
    {
        _cache.Set(key, value, expire ?? Expire.InMinutes(5));
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlForgetTestsExecutor : ForgetTests<MySqlCacheDriver>
{
    public MySqlForgetTestsExecutor(MySqlFixture fixture) : base(fixture)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlForgetTestsExecutor : ForgetTests<PostgreSqlCacheDriver>
{
    public PostgreSqlForgetTestsExecutor(PostgreSqlFixture fixture) : base(fixture)
    {
    }
}