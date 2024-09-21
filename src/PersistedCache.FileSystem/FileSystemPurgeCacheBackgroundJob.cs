using Microsoft.Extensions.Hosting;

namespace PersistedCache;

internal class FileSystemPurgeCacheBackgroundJob : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly IPersistedCache _cache;
    private readonly FileSystemPersistedCacheOptions _options;
    private CancellationToken _cancellationToken;
        
    public FileSystemPurgeCacheBackgroundJob(IPersistedCache cache, FileSystemPersistedCacheOptions options)
    {
        _cache = cache;
        _options = options;
    }
    
    private void PurgeCache(object state)
    {
        _ = _cache.PurgeAsync(_cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _timer = new Timer(PurgeCache, null, TimeSpan.Zero, _options.PurgeInterval);
            
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