using AutoFixture;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class GetOrSetTests : BaseTest
{
    private readonly IPersistedCache _cache;    
    private readonly Fixture _fixture = new();
    public GetOrSetTests(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
        _cache = fixture.PersistedCache;
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