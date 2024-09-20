using System.Text.Json.Serialization;

namespace PersistedCache
{
    internal class PersistedCacheEntry : PersistedCacheEntry<string>;
    
    internal class PersistedCacheEntry<T>
    {
        public required string Key { get; init; }
        public required T Value { get; init; }
        public DateTimeOffset Expiry { get; init; }
        
        [JsonIgnore]
        public bool IsExpired => Expiry < DateTimeOffset.UtcNow;
    }
}