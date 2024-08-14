namespace PersistedCache;

public struct Expire
{
    private readonly DateTimeOffset _value;
    
    private Expire(DateTimeOffset value)
    {
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
    
    public static implicit operator DateTimeOffset(Expire expire) => expire._value;
    public static implicit operator Expire(DateTimeOffset dateTimeOffset) => new Expire(dateTimeOffset);
    public static explicit operator Expire(TimeSpan timeSpan) => new Expire(DateTimeOffset.UtcNow.Add(timeSpan));
}