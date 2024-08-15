namespace PersistedCache.Tests.Fixtures;

public class BaseTest : IAsyncLifetime
{
    private readonly IPersistedCache _cache;

    public BaseTest(IPersistedCache cache)
    {
        _cache = cache;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cache.FlushAsync();
    }
}