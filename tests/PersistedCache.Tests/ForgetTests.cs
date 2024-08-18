using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests
{
    public abstract class ForgetTests : BaseTest
    {
        private readonly IPersistedCache _cache;
        private readonly Fixture _fixture = new Fixture();
    
        protected ForgetTests(IPersistedCache cache) : base(cache)
        {
            _cache = cache;
        }
    
        [Fact]
        public void Forget_WithKey_RemovesValue()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            var value = _fixture.Create<RandomObject>();
            Arrange(key, value);
        
            // Act
            _cache.Forget(key);
        
            // Assert
            var result = _cache.Get<RandomObject>(key);
            result.Should().BeNull();
        }
    
        [Fact]
        public void Forget_WithNonExistingKey_DoesNotThrow()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
        
            // Act
            Action act = () => _cache.Forget(key);
        
            // Assert
            act.Should().NotThrow();
        }
    
        [Fact]
        public async Task ForgetAsync_WithKey_RemovesValue()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            var value = _fixture.Create<RandomObject>();
            Arrange(key, value);
        
            // Act
            await _cache.ForgetAsync(key);
        
            // Assert
            var result = _cache.Get<RandomObject>(key);
            result.Should().BeNull();
        }
    
        private void Arrange<T>(string key, T value, Expire? expire = null)
        {
            _cache.Set(key, value, expire ?? Expire.InMinutes(5));
        }
    }

    [Collection(nameof(MySqlFixture))]
    public class MySqlForgetTestsExecutor : ForgetTests
    {
        public MySqlForgetTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }

    [Collection(nameof(PostgreSqlFixture))]
    public class PostgreSqlForgetTestsExecutor : ForgetTests
    {
        public PostgreSqlForgetTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }
    
    [Collection(nameof(SqlServerFixture))]
    public class SqlServerForgetTestsExecutor : ForgetTests
    {
        public SqlServerForgetTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }
}