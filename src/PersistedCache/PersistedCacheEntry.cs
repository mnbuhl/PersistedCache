﻿using System;

namespace PersistedCache
{
    internal class PersistedCacheEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTimeOffset Expiry { get; set; }
    }
}