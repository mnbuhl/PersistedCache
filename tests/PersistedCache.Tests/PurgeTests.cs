using System;
using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests
{
    public abstract class PurgeTests : BaseTest
    {
        private readonly IPersistedCache _cache;
        private readonly Fixture _fixture = new Fixture();
        private readonly Func<IEnumerable<object>> _getCacheEntries;
    
        protected PurgeTests(IPersistedCache cache, Func<IEnumerable<object>> getCacheEntries) : base(cache)
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
            _cache.SetForever("key3", _fixture.Create<RandomObject>());
            _cache.SetForever("key4", _fixture.Create<RandomObject>());
        
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
            _cache.SetForever("key1", _fixture.Create<RandomObject>());
            _cache.SetForever("key2", _fixture.Create<RandomObject>());

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
    
    [Collection(nameof(SqlServerFixture))]
    public class SqlServerPurgeTestsExecutor : PurgeTests
    {
        public SqlServerPurgeTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache, fixture.GetCacheEntries)
        {
        }
    }
}