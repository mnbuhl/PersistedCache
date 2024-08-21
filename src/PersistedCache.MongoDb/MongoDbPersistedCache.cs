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

        BsonClassMap.RegisterClassMap<PersistedCacheEntry<string>>(map =>
        {
            map.AutoMap();
            map.MapIdProperty(p => p.Key);
        });

        CreateIndexesIfNotExists();
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, Expire expiry)
    {
        Validators.ValidateKey(key);
        Validators.ValidateValue(value);
        
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
    
    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, Expire expiry, CancellationToken cancellationToken = default)
    {
        Validators.ValidateKey(key);
        Validators.ValidateValue(value);
        
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

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        Validators.ValidateKey(key);
        
        var entry = Collection.Find(x => x.Key == key && x.Expiry > DateTimeOffset.UtcNow).FirstOrDefault();

        if (entry == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        Validators.ValidateKey(key);
        
        var entry = await Collection.Find(x => x.Key == key && x.Expiry > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    /// <inheritdoc />
    public T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)
    {
        Validators.ValidateKey(key);

        var entry = Collection.Find(x => x.Key == key && x.Expiry > DateTimeOffset.UtcNow).FirstOrDefault();

        if (entry != null)
        {
            return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions)!;
        }

        var value = valueFactory();
        Validators.ValidateValue(value);
        
        Set(key, value, expiry);
        return value;
    }

    /// <inheritdoc />
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        var entry = await Collection.Find(x => x.Key == key && x.Expiry > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry != null)
        {
            return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions)!;
        }

        var value = await valueFactory();
        Validators.ValidateValue(value);
        
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }

    /// <inheritdoc />
    public void Forget(string key)
    {
        Validators.ValidateKey(key);
        Collection.DeleteOne(x => x.Key == key);
    }

    /// <inheritdoc />
    public async Task ForgetAsync(string key, CancellationToken cancellationToken = default)
    {
        Validators.ValidateKey(key);
        await Collection.DeleteOneAsync(x => x.Key == key, cancellationToken);
    }

    /// <inheritdoc />
    public T? Pull<T>(string key)
    {
        Validators.ValidateKey(key);
        var entry = Collection.FindOneAndDelete(x => x.Key == key);

        if (entry == null || entry.IsExpired)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    /// <inheritdoc />
    public async Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        Validators.ValidateKey(key);
        var entry = await Collection.FindOneAndDeleteAsync(x => x.Key == key, cancellationToken: cancellationToken);

        if (entry == null || entry.IsExpired)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(entry.Value, _options.JsonOptions);
    }

    /// <inheritdoc />
    public void Flush()
    {
        Collection.DeleteMany(FilterDefinition<PersistedCacheEntry>.Empty);
    }

    /// <inheritdoc />
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await Collection.DeleteManyAsync(FilterDefinition<PersistedCacheEntry>.Empty, cancellationToken);
    }

    /// <inheritdoc />
    public void Flush(string pattern)
    {
        Validators.ValidatePattern(pattern, new PatternValidatorOptions { SupportsRegex = true });
        Collection.DeleteMany(Builders<PersistedCacheEntry>.Filter.Regex(entry => entry.Key, pattern));
    }

    /// <inheritdoc />
    public async Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
    {
        Validators.ValidatePattern(pattern, new PatternValidatorOptions { SupportsRegex = true });
        await Collection.DeleteManyAsync(Builders<PersistedCacheEntry>.Filter.Regex(entry => entry.Key, pattern),
            cancellationToken);
    }

    /// <inheritdoc />
    public void Purge()
    {
        Collection.DeleteMany(x => x.Expiry < DateTimeOffset.UtcNow);
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
}