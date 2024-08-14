using FluentAssertions;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class MySqlSetTests(MySqlFixture fixture)
{
    private readonly IPersistedCache _cache = fixture.PersistedCache;

    [Fact]
    public void Set_WithKeyAndValue_SetsValue()
    {
        // Arrange
        const string key = "key";
        const string value = "value";
        
        // Act
        _cache.Set(key, value, TimeSpan.FromMinutes(5));
        
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
        _cache.Set(key, value, TimeSpan.FromMinutes(5));

        // Assert
        var result = _cache.Get<RandomObject>(key);
        result.Should().BeEquivalentTo(value);
    }
}