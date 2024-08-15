﻿using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class FlushTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    private readonly Func<string, IEnumerable<dynamic>> _executeSql;
    
    public FlushTests(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
        _cache = fixture.PersistedCache;
        _executeSql = fixture.ExecuteSql;
    }
    
    [Fact]
    public void Flush_WithExistingKeys_RemovesAllKeys()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        foreach (var key in keys)
        {
            _cache.SetForever(key, _fixture.Create<RandomObject>());
        }
        
        // Act
        _cache.Flush();
        
        // Assert
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void Flush_WithNonExistingKeys_DoesNothing()
    {
        // Act
        _cache.Flush();
        
        // Assert
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void Flush_WithExistingKeysAndExpiredKeys_RemovesAllKeys()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        foreach (var key in keys)
        {
            _cache.Set(key, _fixture.Create<RandomObject>(), Expire.InMilliseconds(1));
        }
        
        // Act
        Thread.Sleep(2);
        _cache.Flush();
        
        // Assert
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task FlushAsync_WithExistingKeys_RemovesAllKeys()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        foreach (var key in keys)
        {
            await _cache.SetForeverAsync(key, _fixture.Create<RandomObject>());
        }
        
        // Act
        await _cache.FlushAsync();
        
        // Assert
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void Flush_WithPattern_RemovesKeysMatchingPattern()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3", "4yek" };
        foreach (var key in keys)
        {
            _cache.SetForever(key, _fixture.Create<RandomObject>());
        }
        
        // Act
        _cache.Flush("key*");
        
        // Assert
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
        result.Should().HaveCount(1);
    }
    
    [Fact]
    public void Flush_WithInvalidPattern_ThrowsException()
    {
        // Act
        Action act = () => _cache.Flush("hello*test");
        
        // Assert
        act.Should().Throw<ArgumentException>();
    }
}