using System;
using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests;

public abstract class PurgeTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new Fixture();
    private readonly Func<IEnumerable<CacheEntry>> _getCacheEntries;
    
    protected PurgeTests(IPersistedCache cache, Func<IEnumerable<CacheEntry>> getCacheEntries) : base(cache)
    {
        _cache = cache;
        _getCacheEntries = getCacheEntries;
    }
    
    [Fact]
    public void Purge_RemovesExpiredKeys()
    {
        // Arrange
        _cache.Set("key1", _fixture.Create<RandomObject>(), Expire.InSeconds(1));
        _cache.Set("key2", _fixture.Create<RandomObject>(), Expire.InSeconds(1));
        _cache.Set("key3", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key4", _fixture.Create<RandomObject>(), Expire.Never);
        
        Thread.Sleep(2000);
        
        // Act
        _cache.Purge();
        
        // Assert
        var result = _getCacheEntries();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Purge_WhenNoExpiredKeys_RemovesNothing()
    {
        // Arrange
        _cache.Set("key1", _fixture.Create<RandomObject>(), Expire.Never);
        _cache.Set("key2", _fixture.Create<RandomObject>(), Expire.Never);

        // Act
        _cache.Purge();

        // Assert
        var result = _getCacheEntries();
        result.Should().HaveCount(2);
    }
}

[Collection(nameof(MySqlFixture))]
public class MySqlPurgeTestsExecutor : PurgeTests
{
    public MySqlPurgeTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
    {
    }
}

[Collection(nameof(PostgreSqlFixture))]
public class PostgreSqlPurgeTestsExecutor : PurgeTests
{
    public PostgreSqlPurgeTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
    {
    }
}
    
// [Collection(nameof(SqlServerFixture))]
// public class SqlServerPurgeTestsExecutor : PurgeTests
// {
//     public SqlServerPurgeTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
//     {
//     }
// }
    
[Collection(nameof(FileSystemFixture))]
public class FileSystemPurgeTestsExecutor : PurgeTests
{
    public FileSystemPurgeTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
    {
    }
}
    
[Collection(nameof(SqliteFixture))]
public class SqlitePurgeTestsExecutor : PurgeTests
{
    public SqlitePurgeTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
    {
    }
}

[Collection(nameof(MongoDbFixture))]
public class MongoDbPurgeTestsExecutor : PurgeTests
{
    public MongoDbPurgeTestsExecutor(MongoDbFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
    {
    }
}