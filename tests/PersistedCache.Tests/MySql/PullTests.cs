using AutoFixture;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class PullTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    private readonly Func<string, IEnumerable<dynamic>> _executeSql;
    
    public PullTests(MySqlFixture fixture) : base(fixture.PersistedCache)
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