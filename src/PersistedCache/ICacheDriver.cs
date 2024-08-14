namespace PersistedCache
{
    public interface ICacheDriver
    {
        string SetupStorageScript { get; }
        string GetScript { get; }
        string SetScript { get; }
        string ForgetScript { get; }
        string FlushScript { get; }
    }
}