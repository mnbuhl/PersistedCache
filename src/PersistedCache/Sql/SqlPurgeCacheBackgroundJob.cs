using Microsoft.Extensions.Hosting;

namespace PersistedCache.Sql
{
    public class SqlPurgeCacheBackgroundJob<TDriver> : IHostedService, IDisposable where TDriver : class, IDriver
    {
        private Timer? _timer;
        private readonly IPersistedCache<TDriver> _cache;
        private readonly SqlPersistedCacheOptions _options;
        private CancellationToken _cancellationToken;
        
        public SqlPurgeCacheBackgroundJob(IPersistedCache<TDriver> cache, SqlPersistedCacheOptions options)
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
}