namespace PersistedCache;

public readonly struct Expire
{
    private readonly DateTimeOffset _value;
    
    private Expire(DateTimeOffset value)
    {
        _value = value;
    }

    private static Expire Create(DateTimeOffset value)
    {
        if (value < DateTimeOffset.UtcNow.AddSeconds(-1))
        {
            throw new ArgumentException("The expire value must be in the future.", nameof(value));
        }
        
        return new Expire(value);
    }
    
    public static Expire Never => Create(DateTimeOffset.MaxValue);
    public static Expire InMilliseconds(int milliseconds) => Create(DateTimeOffset.UtcNow.AddMilliseconds(milliseconds));
    public static Expire InSeconds(int seconds) => Create(DateTimeOffset.UtcNow.AddSeconds(seconds));
    public static Expire InMinutes(int minutes) => Create(DateTimeOffset.UtcNow.AddMinutes(minutes));
    public static Expire InHours(int hours) => Create(DateTimeOffset.UtcNow.AddHours(hours));
    public static Expire InDays(int days) => Create(DateTimeOffset.UtcNow.AddDays(days));
    public static Expire InMonths(int months) => Create(DateTimeOffset.UtcNow.AddMonths(months));
    public static Expire InYears(int years) => Create(DateTimeOffset.UtcNow.AddYears(years));
    public static Expire At(DateTimeOffset dateTime) => Create(dateTime);
    public static Expire In(TimeSpan timeSpan) => Create(DateTimeOffset.UtcNow.Add(timeSpan));
    
    public static implicit operator DateTimeOffset(Expire expire) => expire._value;
    public static implicit operator Expire(DateTimeOffset dateTimeOffset) => new Expire(dateTimeOffset);
    public static explicit operator Expire(TimeSpan timeSpan) => new Expire(DateTimeOffset.UtcNow.Add(timeSpan));
    
    public override string ToString() => _value.ToString();
}