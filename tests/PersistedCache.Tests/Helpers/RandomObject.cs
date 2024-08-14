namespace PersistedCache.Tests.Helpers;

public class RandomObject
{
    public string RandomValue { get; set; } = Guid.NewGuid().ToString();
    public int RandomNumber { get; set; } = new Random().Next();
    public DateTime RandomDateTime { get; set; } = DateTime.Now.AddMinutes(new Random().Next(1, 100));
    public bool RandomBoolean { get; set; } = new Random().Next(0, 1) == 1;
    public double RandomDouble { get; set; } = new Random().NextDouble();
    public decimal RandomDecimal { get; set; } = new Random().Next(1, 100);
    public Guid RandomGuid { get; set; } = Guid.NewGuid();
    public TimeSpan RandomTimeSpan { get; set; } = TimeSpan.FromMinutes(new Random().Next(1, 100));
}