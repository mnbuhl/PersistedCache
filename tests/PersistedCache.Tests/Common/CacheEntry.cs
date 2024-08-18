using System;

namespace PersistedCache.Tests.Common
{
    public class CacheEntry
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Expiry { get; set; }
    }
}