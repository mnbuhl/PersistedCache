using System;
using System.Threading;
using System.Threading.Tasks;

namespace PersistedCache
{
    public interface IPersistedCache
    {
        void Set<T>(string key, T value, TimeSpan expiry);
        void SetForever<T>(string key, T value);
        Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);
        Task SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan expiry);
        T GetOrSetForever<T>(string key, Func<T> valueFactory);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expiry, CancellationToken cancellationToken = default);
        Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory, CancellationToken cancellationToken = default);
    }
}