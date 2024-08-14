using System;
using System.Threading;
using System.Threading.Tasks;

namespace PersistedCache
{
    public interface IPersistedCache
    {
        void Set<T>(string key, T value, TimeSpan expiry);
        Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    }
}