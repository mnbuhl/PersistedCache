namespace PersistedCache.FileSystem;

internal class FileSystemCacheEntry<T>
{
    public required string Key { get; set; }
    public required T Value { get; set; }
    public Expire Expiry { get; set; }
}