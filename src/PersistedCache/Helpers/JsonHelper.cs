using System.Text.Json;

namespace PersistedCache.Helpers;

public class JsonHelper
{
    public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
    {
        try
        {
            if (string.IsNullOrEmpty(json))
            {
                result = default;
                return false;
            }
            
            result = JsonSerializer.Deserialize<T?>(json, options);
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}