namespace PersistedCache
{
    internal class PersistedCacheEntry
    {
        public required string Key { get; init; }
        public required string Value { get; init; }
        public DateTimeOffset Expiry { get; init; }
    }
}