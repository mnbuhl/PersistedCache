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

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _cache.FlushAsync();
    }
}