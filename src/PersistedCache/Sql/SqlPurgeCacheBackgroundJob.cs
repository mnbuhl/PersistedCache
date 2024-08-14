using Microsoft.Extensions.Hosting;

namespace PersistedCache.Sql
{
    public class SqlPurgeCacheBackgroundJob : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IPersistedCache _cache;
        private readonly SqlPersistedCacheOptions _options;
        
        public SqlPurgeCacheBackgroundJob(IPersistedCache cache, SqlPersistedCacheOptions options)
        {
            _cache = cache;
            _options = options;
        }
        
        private void PurgeCache(object state)
        {
            _cache.Purge();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
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