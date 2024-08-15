using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PersistedCache.MySql;
using PersistedCache.PostgreSql;
using PersistedCache.Sql;
using PersistedCache.Tests.Common;
using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;
using Xunit;

namespace PersistedCache.Tests
{
    public abstract class SetTests<TDriver> : BaseTest where TDriver : ISqlCacheDriver
    {
        private readonly IPersistedCache _cache;
        private readonly Fixture _fixture = new Fixture();

        public SetTests(BaseDatabaseFixture<TDriver> fixture) : base(fixture.PersistedCache)
        {
            _cache = fixture.PersistedCache;
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
    public class MySqlSetTestsExecutor : SetTests<MySqlCacheDriver>
    {
        public MySqlSetTestsExecutor(MySqlFixture fixture) : base(fixture)
        {
        }
    }

    [Collection(nameof(PostgreSqlFixture))]
    public class PostgreSqlSetTestsExecutor : SetTests<PostgreSqlCacheDriver>
    {
        public PostgreSqlSetTestsExecutor(PostgreSqlFixture fixture) : base(fixture)
        {
        }
    }
}