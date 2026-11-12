using System;
using System.Globalization;
using Oathx.Entity;
using UnityEngine;

/// <summary>Owns the local trial's announcement reminder preference.</summary>
public class NoticeEntity : XEntity
{
    /// <summary>UTC expiration of automatic announcement suppression.</summary>
    public long suppressedUntil
    { get; private set; }

    /// <summary>Restore the local reminder expiration.</summary>
    public NoticeEntity()
    {
        string key = StorageKey.For<NoticeSystem, NoticeEntity>(entity => entity.suppressedUntil);
        long.TryParse(PlayerPrefs.GetString(key, string.Empty), out long expiration);
        suppressedUntil = expiration;
    }

    /// <summary>Persist the reminder preference without affecting manual opening.</summary>
    /// <param name="selected">Whether automatic opening should be suppressed for seven days.</param>
    public void SetSuppressed(bool selected)
    {
        suppressedUntil = selected ? DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds() : 0;

        string key = StorageKey.For<NoticeSystem, NoticeEntity>(entity => entity.suppressedUntil);
        PlayerPrefs.SetString(key, suppressedUntil.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }
}
