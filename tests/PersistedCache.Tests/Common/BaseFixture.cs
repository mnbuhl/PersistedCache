using System.Collections.Generic;

namespace PersistedCache.Tests.Common;

public abstract class BaseFixture
{
    public abstract IPersistedCache PersistedCache { get; protected set; }
        
    public abstract IEnumerable<CacheEntry> GetCacheEntries();

    public abstract CacheEntry GetCacheEntry(string key);
}