using Microsoft.Extensions.Hosting;

namespace PersistedCache;

public class MongoDbPurgeCacheBackgroundJob : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly IPersistedCache _cache;
    private readonly MongoDbPersistedCacheOptions _options;
        
    public MongoDbPurgeCacheBackgroundJob(IPersistedCache cache, MongoDbPersistedCacheOptions options)
    {
        _cache = cache;
        _options = options;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(_ => _cache.Purge(), null, TimeSpan.Zero, _options.PurgeInterval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}