using System.Text.Json;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace PersistedCache;

internal class MongoDbPersistedCache : IPersistedCache<MongoDbDriver>
{
    private readonly MongoDbPersistedCacheOptions _options;
    private readonly MongoClient _client;

    public MongoDbPersistedCache(MongoDbPersistedCacheOptions options)
    {
        _options = options;
        _client = new MongoClient(options.ConnectionString);

        BsonSerializer.RegisterSerializer(new ExpireBsonSerializer());
        BsonClassMap.RegisterClassMap<PersistedCacheEntry<string>>(map =>
        {
            map.AutoMap();
            map.MapIdProperty(p => p.Key);
        });

        CreateIndexesIfNotExists();
    }

    public void Set<T>(string key, T value, Expire expiry)
    {
        var entry = new PersistedCacheEntry
        {
            Key = key,
            Value = JsonSerializer.Serialize(value, _options.JsonOptions),
            Expiry = expiry
        };

        Collection.FindOneAndReplace<PersistedCacheEntry>(
            filter: x => x.Key == key,
            replacement: entry,
            options: new FindOneAndReplaceOptions<PersistedCacheEntry> { IsUpsert = true }
        );
    }

    public void SetForever<T>(string key, T value)
    {
        Set(key, value, Expire.Never);
    }

    public async Task SetAsync<T>(string key, T value, Expire expiry, CancellationToken cancellationToken = default)
    {
        await Collection.FindOneAndReplaceAsync<PersistedCacheEntry>(
            filter: x => x.Key == key,
            replacement: new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = expiry
            },
            options: new FindOneAndReplaceOptions<PersistedCacheEntry> { IsUpsert = true },
            cancellationToken: cancellationToken
        );
    }

    public async Task SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        await SetAsync(key, value, Expire.Never, cancellationToken);
    }

    public T? Get<T>(string key)
    {
        var entry = Collection.Find(x => x.Key == key && x.Expiry > Expire.Now).FirstOrDefault();

        if (entry == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var entry = await Collection.Find(x => x.Key == key && x.Expiry > Expire.Now)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)
    {
        var entry = Collection.Find(x => x.Key == key && x.Expiry > Expire.Now).FirstOrDefault();

        if (entry != null)
        {
            return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions)!;
        }

        var value = valueFactory();
        Set(key, value, expiry);
        return value;
    }

    public T GetOrSetForever<T>(string key, Func<T> valueFactory)
    {
        return GetOrSet(key, valueFactory, Expire.Never);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        var entry = await Collection.Find(x => x.Key == key && x.Expiry > Expire.Now)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry != null)
        {
            return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions)!;
        }

        var value = await valueFactory();
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }

    public async Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory,
        CancellationToken cancellationToken = default)
    {
        return await GetOrSetAsync(key, valueFactory, Expire.Never, cancellationToken);
    }

    public void Forget(string key)
    {
        Collection.DeleteOne(x => x.Key == key);
    }

    public async Task ForgetAsync(string key, CancellationToken cancellationToken = default)
    {
        await Collection.DeleteOneAsync(x => x.Key == key, cancellationToken);
    }

    public T? Pull<T>(string key)
    {
        var entry = Collection.FindOneAndDelete(x => x.Key == key);

        if (entry == null || entry.Expiry.IsExpired)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    public async Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var entry = await Collection.FindOneAndDeleteAsync(x => x.Key == key, cancellationToken: cancellationToken);

        if (entry == null || entry.Expiry.IsExpired)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    public void Flush()
    {
        Collection.DeleteMany(FilterDefinition<PersistedCacheEntry>.Empty);
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await Collection.DeleteManyAsync(FilterDefinition<PersistedCacheEntry>.Empty, cancellationToken);
    }

    public void Flush(string pattern)
    {
        ValidatePattern(pattern);
        Collection.DeleteMany(x => x.Key == pattern);
    }

    public async Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ValidatePattern(pattern);
        await Collection.DeleteManyAsync(x => x.Key == pattern, cancellationToken);
    }

    public void Purge()
    {
        Collection.DeleteMany(x => x.Expiry < Expire.Now);
    }

    private void CreateIndexesIfNotExists()
    {
        var expiryIndex = new CreateIndexModel<PersistedCacheEntry>(
            Builders<PersistedCacheEntry>.IndexKeys.Ascending(entry => entry.Expiry)
        );

        var keyExpiryIndex = new CreateIndexModel<PersistedCacheEntry>(
            Builders<PersistedCacheEntry>.IndexKeys
                .Ascending(entry => entry.Key)
                .Ascending(entry => entry.Expiry)
        );

        Collection.Indexes.CreateMany([expiryIndex, keyExpiryIndex]);
    }

    private IMongoCollection<PersistedCacheEntry> Collection => _client.GetDatabase(_options.DatabaseName)
        .GetCollection<PersistedCacheEntry>(_options.CollectionName);
    
    private static void ValidatePattern(string pattern)
    {
        if (pattern.Contains("*") || pattern.Contains("?"))
        {
            throw new NotSupportedException("Pattern matching is not supported by this cache driver.");
        }
    }
}