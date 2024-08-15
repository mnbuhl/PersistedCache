namespace PersistedCache.Tests.Helpers;

public class RandomNestedObject
{
    public string RandomValue { get; set; } = string.Empty;
    public int RandomNumber { get; set; }
    public DateTime RandomDateTime { get; set; }
    public bool RandomBoolean { get; set; }
    public Guid RandomGuid { get; set; }
    public TimeSpan RandomTimeSpan { get; set; }
}