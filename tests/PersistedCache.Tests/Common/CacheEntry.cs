namespace PersistedCache.Tests.Common;

public class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
}