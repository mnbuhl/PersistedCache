namespace PersistedCache.Tests.Fixtures;

public class BaseTest(IPersistedCache cache) : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        cache.Purge();
        return Task.CompletedTask;
    }
}