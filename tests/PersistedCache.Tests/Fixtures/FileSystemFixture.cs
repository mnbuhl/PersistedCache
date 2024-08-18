using System.Collections.Generic;
using System.IO;
using PersistedCache.FileSystem;
using PersistedCache.Tests.Common;

namespace PersistedCache.Tests.Fixtures
{
    public class FileSystemFixture : BaseFixture
    {
        public sealed override IPersistedCache PersistedCache { get; protected set; }

        public FileSystemFixture()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "cache");
            var options = new FileSystemPersistedCacheOptions(path);
            PersistedCache = new FileSystemPersistedCache(options);
        }
        
        public override IEnumerable<CacheEntry> GetCacheEntries()
        {
            throw new System.NotImplementedException();
        }

        public override CacheEntry GetCacheEntry(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}