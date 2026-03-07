using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class GetOrSetTests : BaseTest
{
    private readonly IPersistedCache _cache;    
    private readonly Fixture _fixture = new Fixture();
    private readonly Func<string, CacheEntry> _getCacheEntry;
    
    protected GetOrSetTests(IPersistedCache cache, Func<string, CacheEntry> getCacheEntry) : base(cache)
    {
        _cache = cache;
        _getCacheEntry = getCacheEntry;
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
        
        var newValue = _fixture.Create<RandomObject>();

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
        var result = await _cache.GetOrSetAsync(key, () => value, Expire.InMinutes(5));

        // Assert
        result.Should().BeEquivalentTo(value);
    }
    
    [Fact]
    public async Task GetOrSetAsync_WithAsyncValueFactory_ReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = await _cache.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(100);
            return value;
        }, Expire.InMinutes(5));

        // Assert
        result.Should().BeEquivalentTo(value);
    }
    
    [Fact]
    public async Task GetOrSetAsync_WithAsyncValueFactoryAndCancellationToken_ReturnsValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = await _cache.GetOrSetAsync(key, async ct =>
        {
            await Task.Delay(100, ct);
            return value;
        }, Expire.InMinutes(5), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void GetOrSet_WithOptionsAndExpiryOverwritten_StoresValueWithCustomExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        var expectedExpiry = Expire.InMinutes(10);

        // Act
        var result = _cache.GetOrSet(key, options =>
        {
            options.Expiry = expectedExpiry;
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry is correct
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetOrSet_WithOptionsAndExpiryNotOverwritten_StoresValueWithDefaultExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = _cache.GetOrSet(key, _ => value);

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry defaults to Never (MaxValue)
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(DateTimeOffset.MaxValue, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetOrSet_WithOptionsWhenValueExists_ReturnsExistingValueWithoutCallingFactory()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var oldValue = _fixture.Create<RandomObject>();
        Arrange(key, oldValue);
        
        var newValue = _fixture.Create<RandomObject>();
        var factoryCalled = false;

        // Act
        var result = _cache.GetOrSet(key, options =>
        {
            factoryCalled = true;
            options.Expiry = Expire.InMinutes(10);
            return newValue;
        });

        // Assert
        result.Should().BeEquivalentTo(oldValue);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public void GetOrSet_WithOptionsWhenKeyDoesNotExist_CallsFactoryAndStoresValue()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        var factoryCalled = false;

        // Act
        var result = _cache.GetOrSet(key, options =>
        {
            factoryCalled = true;
            options.Expiry = Expire.InHours(2);
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        factoryCalled.Should().BeTrue();
        
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task GetOrSetAsync_WithOptionsAndExpiryOverwritten_StoresValueWithCustomExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        var expectedExpiry = Expire.InMinutes(10);

        // Act
        var result = await _cache.GetOrSetAsync(key, options =>
        {
            options.Expiry = expectedExpiry;
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry is correct
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetOrSetAsync_WithOptionsAndExpiryNotOverwritten_StoresValueWithDefaultExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = await _cache.GetOrSetAsync(key, _ => value);

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry defaults to Never (MaxValue)
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(DateTimeOffset.MaxValue, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetOrSetAsync_WithOptionsWhenValueExists_ReturnsExistingValueWithoutCallingFactory()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var oldValue = _fixture.Create<RandomObject>();
        Arrange(key, oldValue);
        
        var newValue = _fixture.Create<RandomObject>();
        var factoryCalled = false;

        // Act
        var result = await _cache.GetOrSetAsync(key, options =>
        {
            factoryCalled = true;
            options.Expiry = Expire.InMinutes(10);
            return newValue;
        });

        // Assert
        result.Should().BeEquivalentTo(oldValue);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncOptionsAndExpiryOverwritten_StoresValueWithCustomExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        var expectedExpiry = Expire.InMinutes(15);

        // Act
        var result = await _cache.GetOrSetAsync(key, async options =>
        {
            await Task.Delay(100);
            options.Expiry = expectedExpiry;
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry is correct
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncOptionsAndExpiryNotOverwritten_StoresValueWithDefaultExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = await _cache.GetOrSetAsync(key, async _ =>
        {
            await Task.Delay(100);
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry defaults to Never (MaxValue)
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(DateTimeOffset.MaxValue, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncOptionsWhenValueExists_ReturnsExistingValueWithoutCallingFactory()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var oldValue = _fixture.Create<RandomObject>();
        Arrange(key, oldValue);
        
        var newValue = _fixture.Create<RandomObject>();
        var factoryCalled = false;

        // Act
        var result = await _cache.GetOrSetAsync(key, async options =>
        {
            factoryCalled = true;
            await Task.Delay(100);
            options.Expiry = Expire.InMinutes(10);
            return newValue;
        });

        // Assert
        result.Should().BeEquivalentTo(oldValue);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncOptionsWithCancellationTokenAndExpiryOverwritten_StoresValueWithCustomExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();
        var expectedExpiry = Expire.InMinutes(20);

        // Act
        var result = await _cache.GetOrSetAsync(key, async (options, ct) =>
        {
            await Task.Delay(100, ct);
            options.Expiry = expectedExpiry;
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry is correct
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncOptionsWithCancellationTokenAndExpiryNotOverwritten_StoresValueWithDefaultExpiry()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var value = _fixture.Create<RandomObject>();

        // Act
        var result = await _cache.GetOrSetAsync(key, async (_, ct) =>
        {
            await Task.Delay(100, ct);
            return value;
        });

        // Assert
        result.Should().BeEquivalentTo(value);
        
        // Verify value is stored
        var cachedValue = _cache.Get<RandomObject>(key);
        cachedValue.Should().BeEquivalentTo(value);
        
        // Verify expiry defaults to Never (MaxValue)
        var cacheEntry = _getCacheEntry(key);
        cacheEntry.Should().NotBeNull();
        cacheEntry.ExpiryDate.Should().BeCloseTo(DateTimeOffset.MaxValue, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetOrSetAsync_WithAsyncOptionsWithCancellationTokenWhenValueExists_ReturnsExistingValueWithoutCallingFactory()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var oldValue = _fixture.Create<RandomObject>();
        Arrange(key, oldValue);
        
        var newValue = _fixture.Create<RandomObject>();
        var factoryCalled = false;

        // Act
        var result = await _cache.GetOrSetAsync(key, async (options, ct) =>
        {
            factoryCalled = true;
            await Task.Delay(100, ct);
            options.Expiry = Expire.InMinutes(10);
            return newValue;
        });

        // Assert
        result.Should().BeEquivalentTo(oldValue);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WithCancellationToken_RespectsOperationCancellation()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await _cache.GetOrSetAsync(key, async (_, ct) =>
            {
                await cts.CancelAsync();
                await Task.Delay(1000, ct);
                return new RandomObject();
            }, cts.Token);
        });
    }

    private void Arrange<T>(string key, T value, Expire? expire = null)
    {
        _cache.Set(key, value, expire ?? Expire.InMinutes(5));
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlGetOrSetTestsExecutor : GetOrSetTests
{
    public MySqlGetOrSetTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry!)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlGetOrSetTestsExecutor : GetOrSetTests
{
    public PostgreSqlGetOrSetTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry!)
    {
    }
}
    
[Collection(nameof(SqlServerFixture))]
public class SqlServerGetOrSetTestsExecutor : GetOrSetTests
{
    public SqlServerGetOrSetTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry!)
    {
    }
}
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemGetOrSetTestsExecutor : GetOrSetTests
{
    public FileSystemGetOrSetTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry!)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqliteGetOrSetTestsExecutor : GetOrSetTests
{
    public SqliteGetOrSetTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry!)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbGetOrSetTestsExecutor : GetOrSetTests
{
    public MongoDbGetOrSetTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntry!)
    {
    }
}