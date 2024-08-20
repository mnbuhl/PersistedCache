using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PersistedCache.Tests.Common;
using Testcontainers.MongoDb;
using Xunit;

namespace PersistedCache.Tests.Fixtures;

[CollectionDefinition(nameof(MongoDbFixture))]
public class MongoDbFixture : BaseFixture, IAsyncLifetime, ICollectionFixture<MongoDbFixture>
{
    private const string DatabaseName = "persistedCache";
    
    private readonly MongoDbContainer _container = new MongoDbBuilder()
        .WithUsername("root")
        .WithPassword("secret")
        .Build();
    
    private IMongoCollection<PersistedCacheEntry> _collection;
    
    public override IPersistedCache PersistedCache { get; protected set; }
    public override IEnumerable<CacheEntry> GetCacheEntries()
    {
        var entries = _collection.Find(FilterDefinition<PersistedCacheEntry>.Empty).ToList();
        
        return entries.Select(entry => new CacheEntry
        {
            Key = entry.Key,
            Value = entry.Value,
            Expiry = entry.Expiry.ToString()
        });
    }

    public override CacheEntry GetCacheEntry(string key)
    {
        var entry = _collection.Find(entry => entry.Key == key).FirstOrDefault();
        
        return entry == null ? null : new CacheEntry
        {
            Key = entry.Key,
            Value = entry.Value,
            Expiry = entry.Expiry.ToString()
        };
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var connectionString = _container.GetConnectionString();
        var options = new MongoDbPersistedCacheOptions(connectionString, DatabaseName);
        PersistedCache = new MongoDbPersistedCache(options);
        
        _collection = new MongoClient(connectionString)
            .GetDatabase(DatabaseName)
            .GetCollection<PersistedCacheEntry>(options.CollectionName);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}