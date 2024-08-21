using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class SetTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new Fixture();

    protected SetTests(IPersistedCache cache) : base(cache)
    {
        _cache = cache;
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
        var value = _fixture.Create<RandomObject>();

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
            value.Add(_fixture.Create<RandomObject>());
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
        var act = new Action(() => _cache.Set(key, value, Expire.InMinutes(-5)));

        // Assert
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Set_WithInvalidKey_ThrowsArgumentException()
    {
        // Arrange
        const string key = null;
        const string value = "value";

        // Act
        var act = new Action(() => _cache.Set(key, value, Expire.InMinutes(5)));

        // Assert
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Set_InParallel_SetsValues()
    {
        // Arrange
        const int count = 100;
        var keys = new List<string>();
        var values = new List<string>();

        for (var i = 0; i < count; i++)
        {
            keys.Add($"key_{i}");
            values.Add($"value_{i}");
        }

        // Act
        Parallel.For(0, count, i => _cache.Set(keys[i], values[i], Expire.InMinutes(5)));

        // Assert
        for (var i = 0; i < count; i++)
        {
            var result = _cache.Get<string>(keys[i]);
            result.Should().Be(values[i]);
        }
    }
    
    [Fact]
    public void Set_WithTooLongKey_ThrowsArgumentException()
    {
        // Arrange
        var key = "a".PadRight(256, 'a');
        const string value = "value";

        // Act
        var act = new Action(() => _cache.Set(key, value, Expire.InMinutes(5)));

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

[Collection(nameof(MySqlFixture))]
public class MySqlSetTestsExecutor : SetTests
{
    public MySqlSetTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlSetTestsExecutor : SetTests
{
    public PostgreSqlSetTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqlServerFixture))]
public class SqlServerSetTestsExecutor : SetTests
{
    public SqlServerSetTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemSetTestsExecutor : SetTests
{
    public FileSystemSetTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqliteSetTestsExecutor : SetTests
{
    public SqliteSetTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbSetTestsExecutor : SetTests
{
    public MongoDbSetTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache)
    {
    }
}