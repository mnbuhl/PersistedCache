using System.Threading.Tasks;
using Xunit;

namespace PersistedCache.Tests.Common;

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