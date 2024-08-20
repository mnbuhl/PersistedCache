namespace PersistedCache;

internal class PatternValidatorOptions
{
    public string[] SupportedWildcards { get; init; } = ["*"];
    public bool SupportsRegex { get; init; }
}

internal class KeyValidatorOptions
{
    public char[] InvalidChars { get; init; } = [];
    public uint MaxLength { get; init; } = 255;
}

internal class ValueValidatorOptions
{
    public bool AllowValueTypes { get; init; } = true;
}

internal static class Validators
{
    public static void ValidatePattern(string pattern, PatternValidatorOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or empty.");
        }
        
        options ??= new PatternValidatorOptions();

        var startsOrEndsWithWildcard = options.SupportedWildcards.Any(pattern.StartsWith) ||
                                       options.SupportedWildcards.Any(pattern.EndsWith);

        if (!startsOrEndsWithWildcard)
        {
            throw new ArgumentException(
                "Pattern must start or end with a wildcard character. Supported wildcard characters: " +
                options.SupportedWildcards
            );
        }
        
        if (options.SupportsRegex)
        {
            throw new NotImplementedException("Regex pattern validation is not implemented.");
        }
    }

    public static void ValidateKey(string key, KeyValidatorOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or empty.");
        }
        
        options ??= new KeyValidatorOptions();
        
        if (key.Length > options.MaxLength)
        {
            throw new ArgumentException("Key is too long. Maximum length: " + options.MaxLength);
        }

        if (options.InvalidChars.Any(key.Contains))
        {
            throw new ArgumentException("Key contains invalid characters. Invalid characters: " +
                                        string.Join(", ", options.InvalidChars));
        }
    }
    
    public static void ValidateValue<T>(T value, ValueValidatorOptions? options = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null");
        }
        
        options ??= new ValueValidatorOptions();
        
        if (!options.AllowValueTypes && value.GetType().IsValueType)
        {
            throw new ArgumentException("Value type is not allowed.");
        }
    }
}