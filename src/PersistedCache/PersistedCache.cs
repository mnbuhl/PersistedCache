namespace PersistedCache
{
    public class PersistedCache : IPersistedCache
    {
        private readonly ICacheDriver _driver;
        private readonly PersistedCacheOptions _options;

        public PersistedCache(ICacheDriver driver)
        {
            _driver = driver;
        }
    }
}