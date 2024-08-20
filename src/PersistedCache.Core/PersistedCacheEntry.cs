namespace PersistedCache
{
    internal class PersistedCacheEntry : PersistedCacheEntry<string>;
    
    internal class PersistedCacheEntry<T>
    {
        public required string Key { get; init; }
        public required T Value { get; init; }
        public Expire Expiry { get; init; }
    }
}