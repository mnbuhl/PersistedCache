using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PersistedCache.FileSystem;
using PersistedCache.Tests.Common;
using Xunit;

namespace PersistedCache.Tests.Fixtures;

[CollectionDefinition(nameof(FileSystemFixture))]
public class FileSystemFixture : BaseFixture, ICollectionFixture<FileSystemFixture>
{
    public sealed override IPersistedCache PersistedCache { get; protected set; }
    private readonly string _path;

    public FileSystemFixture()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "cache");
        _path = path;
        var options = new FileSystemPersistedCacheOptions(path);
        PersistedCache = new FileSystemPersistedCache(options);
    }
        
    public override IEnumerable<CacheEntry> GetCacheEntries()
    {
        var dirInfo = new DirectoryInfo(_path);
        foreach (var file in dirInfo.GetFiles())
        {
            var fileText = File.ReadAllText(file.FullName);
            yield return JsonSerializer.Deserialize<CacheEntry>(fileText);
        }
    }

    public override CacheEntry GetCacheEntry(string key)
    {
        var filePath = Path.Combine(_path, key);
            
        if (!File.Exists(filePath))
        {
            return null;
        }
            
        var fileText = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<CacheEntry>(fileText);
    }
}