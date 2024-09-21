using System.Collections.Generic;

namespace PersistedCache.Tests.Helpers;

public class CompletelyDifferentObject
{
    public List<string> TestValue { get; set; } = new List<string>();
    public int TestNumber { get; set; }
}