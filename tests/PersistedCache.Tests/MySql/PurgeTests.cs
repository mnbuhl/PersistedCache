using PersistedCache.Tests.Fixtures;
using PersistedCache.Tests.Helpers;

namespace PersistedCache.Tests.MySql;

[Collection(nameof(MySqlFixture))]
public class PurgeTests : BaseTest
{
    private readonly IPersistedCache _cache;
    private readonly Fixture _fixture = new();
    private readonly Func<string, IEnumerable<dynamic>> _executeSql;
    
    public PurgeTests(MySqlFixture fixture) : base(fixture.PersistedCache)
    {
        _cache = fixture.PersistedCache;
        _executeSql = fixture.ExecuteSql;
    }
    
    [Fact]
    public void Purge_RemovesExpiredKeys()
    {
        // Arrange
        _cache.Set("key1", _fixture.Create<RandomObject>(), Expire.InMilliseconds(1));
        _cache.Set("key2", _fixture.Create<RandomObject>(), Expire.InMilliseconds(1));
        _cache.SetForever("key3", _fixture.Create<RandomObject>());
        _cache.SetForever("key4", _fixture.Create<RandomObject>());
        
        Thread.Sleep(2);
        
        // Act
        _cache.Purge();
        
        // Assert
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
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
        var result = _executeSql($"SELECT * FROM {TestConstants.TableName}");
        result.Should().HaveCount(2);
    }
}