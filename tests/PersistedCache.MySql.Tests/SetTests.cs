using FluentAssertions;
using PersistedCache.MySql.Tests.Helpers;

namespace PersistedCache.MySql.Tests;

[Collection(nameof(MySqlFixture))]
public class SetTests(MySqlFixture fixture)
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
}