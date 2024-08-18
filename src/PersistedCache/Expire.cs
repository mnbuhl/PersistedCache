using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersistedCache;

public readonly struct Expire : IEquatable<Expire>, IComparable<Expire>, IComparable
{
    private readonly DateTimeOffset _value;
    
    private Expire(DateTimeOffset value)
    {
        if (value < DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("The expire value must be in the future.", nameof(value));
        }
        
        _value = value;
    }
    
    public static Expire Never => new Expire(DateTimeOffset.MaxValue);
    public static Expire InMilliseconds(int milliseconds) => new Expire(DateTimeOffset.UtcNow.AddMilliseconds(milliseconds));
    public static Expire InSeconds(int seconds) => new Expire(DateTimeOffset.UtcNow.AddSeconds(seconds));
    public static Expire InMinutes(int minutes) => new Expire(DateTimeOffset.UtcNow.AddMinutes(minutes));
    public static Expire InHours(int hours) => new Expire(DateTimeOffset.UtcNow.AddHours(hours));
    public static Expire InDays(int days) => new Expire(DateTimeOffset.UtcNow.AddDays(days));
    public static Expire InMonths(int months) => new Expire(DateTimeOffset.UtcNow.AddMonths(months));
    public static Expire InYears(int years) => new Expire(DateTimeOffset.UtcNow.AddYears(years));
    public static Expire At(DateTimeOffset dateTime) => new Expire(dateTime);
    public static Expire In(TimeSpan timeSpan) => new Expire(DateTimeOffset.UtcNow.Add(timeSpan));
    
    [JsonIgnore]
    public bool IsExpired => _value < DateTimeOffset.UtcNow;
    
    
    
    public static implicit operator DateTimeOffset(Expire expire) => expire._value;
    public static implicit operator Expire(DateTimeOffset dateTimeOffset) => new Expire(dateTimeOffset);
    public static explicit operator Expire(TimeSpan timeSpan) => new Expire(DateTimeOffset.UtcNow.Add(timeSpan));
    
    public override string ToString() => _value.ToString();
    public int CompareTo(object obj)
    {
        if (obj is Expire expire)
        {
            return CompareTo(expire);
        }

        throw new ArgumentException($"Object must be of type {nameof(Expire)}");
    }

    public override bool Equals(object? obj) => obj is Expire expire && _value == expire._value;
    public override int GetHashCode() => _value.GetHashCode();
    
    public static bool operator ==(Expire left, Expire right) => left.Equals(right);
    public static bool operator !=(Expire left, Expire right) => !left.Equals(right);

    public bool Equals(Expire other)
    {
        return _value.Equals(other._value);
    }

    public int CompareTo(Expire other)
    {
        return _value.CompareTo(other._value);
    }
}

internal class ExpireJsonConverter : JsonConverter<Expire>
{
    public override Expire Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, Expire value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}