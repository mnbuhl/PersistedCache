using System;
using System.Globalization;

namespace PersistedCache.Tests.Common;

public class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;

    public DateTimeOffset ExpiryDate =>
        DateTimeOffset.ParseExact(Expiry, [
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss zzz",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss.fffffffzzz",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
                "dd/MM/yyyy HH.mm.ss",
                "dd/MM/yyyy HH.mm.ss zzz",
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyy-MM-ddTHH:mm:ss.fffffffK"
            ], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal
        );
}