using AutoFixture;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class ForgetTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    
    public ForgetTests(MySqlFixture fixture) : base(fixture.PersistedCache)
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