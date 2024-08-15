﻿using AutoFixture;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class GetTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    
    public GetTests(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
        _cache = fixture.PersistedCache;
    }
    
    [Theory]
    [InlineData("string", "value")]
    [InlineData("number", 1)]
    [InlineData("bool", true)]
    public void Get_WithKeyAndValue_ReturnsValue<T>(string key, T value)
    {
        // Arrange
        Arrange(key, value);
        
        // Act
        var result = _cache.Get<T>(key);
        
        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Get_WithKeyAndObjectValue_ReturnsValue()
    {
        // Arrange
        const string key = "random_object";
        var value = _fixture.Create<RandomObject>();
        Arrange(key, value);
        
        // Act
        var result = _cache.Get<RandomObject>(key);
        
        // Assert
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void Get_WithNonExistingKey_ReturnsNull()
    {
        // Arrange
        const string key = "non_existing_key";
        
        // Act
        var result = _cache.Get<string>(key);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Theory]
    [InlineData("value")]
    [InlineData(1)]
    [InlineData(true)]
    public async Task Get_WithMissingKeyAndValueType_ReturnsDefaultValue<T>(T value)
    {
        // Arrange
        Arrange(Guid.NewGuid().ToString(), value);
        
        // Act
        var result = await _cache.GetAsync<T>(Guid.NewGuid().ToString());
        
        // Assert
        result.Should().Be(default(T));
    }
    
    [Fact]
    public async Task GetAsync_WithKeyAndValue_ReturnsValue()
    {
        // Arrange
        const string key = "string";
        const string value = "value";
        Arrange(key, value);
        
        // Act
        var result = await _cache.GetAsync<string>(key);
        
        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_WithExpiredKey_ReturnsNull()
    {
        // Arrange
        const string key = "expired";
        const string value = "value";
        Arrange(key, value, Expire.InMilliseconds(1));
        
        // Act
        await Task.Delay(2);
        
        // Assert
        var result = _cache.Get<string>(key);
        result.Should().BeNull();
    }

    private void Arrange<T>(string key, T value, Expire? expire = null)
    {
        _cache.Set(key, value, expire ?? Expire.InMinutes(5));
    }
}