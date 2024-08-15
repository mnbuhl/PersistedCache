using FluentAssertions;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class SetTests
{
    private readonly IPersistedCache _cache;

    public SetTests(MySqlFixture fixture)
    {
        _cache = fixture.PersistedCache;
    }

    [Fact]
    public void Set_WithKeyAndValue_SetsValue()
    {
        // Arrange
        const string key = "key";
        const string value = "value";
        
        // Act
        _cache.Set(key, value, Expire.InMinutes(5));
        
        // Assert
        var result = _cache.Get<string>(key);
        result.Should().Be(value);
    }

    [Fact]
    public void Set_WithKeyAndObjectValue_SetsValue()
    {
        // Arrange
        const string key = "random_object";
        var value = new RandomObject();

        // Act
        _cache.Set(key, value, Expire.InMinutes(5));

        // Assert
        var result = _cache.Get<RandomObject>(key);
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void Set_WithKeyAndArrayValue_SetsValue()
    {
        // Arrange
        const string key = "array";
        var value = new List<RandomObject>();
        
        for (var i = 0; i < 100; i++)
        {
            value.Add(new RandomObject());
        }
        
        // Act
        _cache.Set(key, value, Expire.InMinutes(5));
        
        // Assert
        var result = _cache.Get<List<RandomObject>>(key);
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void Set_ForExistingKeyWithNewValue_UpdatesValue()
    {
        // Arrange
        const string key = "key";
        const string value = "value";
        const string newValue = "new_value";

        // Act
        _cache.Set(key, value, Expire.InMinutes(5));
        _cache.Set(key, newValue, Expire.InMinutes(5));

        // Assert
        var result = _cache.Get<string>(key);
        result.Should().Be(newValue);
    }

    [Fact]
    public void Set_WithInvalidExpire_ThrowsArgumentException()
    {
        // Arrange
        const string key = "key";
        const string value = "value";

        // Act
        var act = () => _cache.Set(key, value, Expire.InMinutes(-5));

        // Assert
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Set_WithInvalidKey_ThrowsArgumentException()
    {
        // Arrange
        const string? key = null;
        const string value = "value";

        // Act
        var act = () => _cache.Set(key!, value, Expire.InMinutes(5));

        // Assert
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Set_WithTooLongKey_ThrowsArgumentException()
    {
        // Arrange
        var key = "a".PadRight(256, 'a');
        const string value = "value";

        // Act
        var act = () => _cache.Set(key, value, Expire.InMinutes(5));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task SetAsync_WithKeyAndValue_SetsValue()
    {
        // Arrange
        const string key = "key";
        const string value = "value";

        // Act
        await _cache.SetAsync(key, value, Expire.InMinutes(5));

        // Assert
        var result = await _cache.GetAsync<string>(key);
        result.Should().Be(value);
    }
}