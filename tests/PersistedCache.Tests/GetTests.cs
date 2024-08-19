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
    public abstract class GetTests : BaseTest
    {
        private readonly IPersistedCache _cache;
        private readonly Fixture _fixture = new Fixture();
    
        protected GetTests(IPersistedCache cache) : base(cache)
        {
            _cache = cache;
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
            Arrange(key, value, Expire.InSeconds(1));
        
            // Act
            await Task.Delay(2000);
        
            // Assert
            var result = await _cache.GetAsync<string>(key);
            result.Should().BeNull();
        }

        private void Arrange<T>(string key, T value, Expire? expire = null)
        {
            _cache.Set(key, value, expire ?? Expire.InMinutes(5));
        }
    }

    [Collection(nameof(MySqlFixture))]
    public class MySqlGetTestsExecutor : GetTests
    {
        public MySqlGetTestsExecutor(MySqlFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }

    [Collection(nameof(PostgreSqlFixture))]
    public class PostgreSqlGetTestsExecutor : GetTests
    {
        public PostgreSqlGetTestsExecutor(PostgreSqlFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }
    
    [Collection(nameof(SqlServerFixture))]
    public class SqlServerGetTestsExecutor : GetTests
    {
        public SqlServerGetTestsExecutor(SqlServerFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }
    
    [Collection(nameof(FileSystemFixture))]
    public class FileSystemGetTestsExecutor : GetTests
    {
        public FileSystemGetTestsExecutor(FileSystemFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }
    
    [Collection(nameof(SqliteFixture))]
    public class SqliteGetTestsExecutor : GetTests
    {
        public SqliteGetTestsExecutor(SqliteFixture fixture) : base(fixture.PersistedCache)
        {
        }
    }
}