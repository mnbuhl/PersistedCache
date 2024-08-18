namespace PersistedCache.FileSystem;

internal class FileSystemCacheEntry<T>
{
    public required string Key { get; init; }
    public required T Value { get; init; }
    public Expire Expiry { get; init; }
}