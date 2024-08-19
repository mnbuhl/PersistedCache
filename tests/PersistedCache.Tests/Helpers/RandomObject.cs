using System;

namespace PersistedCache.Tests.Helpers;

public class RandomObject
{
    public string RandomValue { get; set; } = string.Empty;
    public int RandomNumber { get; set; }
    public DateTime RandomDateTime { get; set; }
    public bool RandomBoolean { get; set; }
    public Guid RandomGuid { get; set; } 
    public TimeSpan RandomTimeSpan { get; set; }
    public RandomNestedObject NestedObject { get; set; } = new RandomNestedObject();
}