namespace PersistedCache
{
    internal class PersistedCacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset Expiry { get; set; }
    }
}