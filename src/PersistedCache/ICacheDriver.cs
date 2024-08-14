﻿using System.Data.Common;

namespace PersistedCache
{
    public interface ICacheDriver
    {
        string SetupStorageScript { get; }
        string GetScript { get; }
        string SetScript { get; }
        string ForgetScript { get; }
        string FlushScript { get; }
        string FlushPatternScript { get; }
        char Wildcard { get; }
        DbConnection CreateConnection();
    }
}