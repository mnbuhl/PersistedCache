using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.MySql;
using PersistedCache.PostgreSql;
using PersistedCache.Sql;
using PersistedCache.SqlServer;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests
{
    public abstract class FlushTests : BaseTest
    {
        private readonly IPersistedCache _cache;
        private readonly Fixture _fixture = new Fixture();
        private readonly Func<string, IEnumerable<dynamic>> _executeSql;
    
        public FlushTests(IPersistedCache cache, Func<string, IEnumerable<dynamic>> executeSql) : base(cache)
        {
            _cache = cache;
            _executeSql = executeSql;
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
                _cache.Set(key, _fixture.Create<RandomObject>(), Expire.InSeconds(1));
            }
        
            // Act
            Thread.Sleep(2000);
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

    [Collection(nameof(MySqlFixture))]
    public class MySqlFlushTestsExecutor : FlushTests
    {
        public MySqlFlushTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache, fixture.ExecuteSql)
        {
        }
    }

    [Collection(nameof(PostgreSqlFixture))]
    public class PostgreSqlFlushTestsExecutor : FlushTests
    {
        public PostgreSqlFlushTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache, fixture.ExecuteSql)
        {
        }
    }
    
    [Collection(nameof(SqlServerFixture))]
    public class SqlServerFlushTestsExecutor : FlushTests
    {
        public SqlServerFlushTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache, fixture.ExecuteSql)
        {
        }
    }
}
