using System;
using System.Collections.Generic;
using System.Linq;
using Oathx.Entity;

/// <summary>Owns local inbox state and validates every mail operation.</summary>
[Initialize]
public partial class MailEntity : XEntity
{
    /// <summary>Private inbox, detached from caller-owned records.</summary>
    private readonly List<MailMessage> messages = new List<MailMessage>();

    /// <summary>Local claim receipts; these are not inventory grants.</summary>
    private readonly List<MailMessage> receipts = new List<MailMessage>();

    /// <summary>Whether the host has supplied an inbox, including an empty inbox.</summary>
    public bool initialized
    { get; private set; }

    /// <summary>Selected message identifier, or zero when nothing is selected.</summary>
    public int selectedId
    { get; private set; }

    /// <summary>Atomically replace the inbox after validating and copying all records.</summary>
    /// <param name="source">Host records to copy.</param>
    /// <returns>True when every record is valid.</returns>
    public bool ReplaceInbox(IEnumerable<MailMessage> source)
    {
        if (source == null)
            return false;

        var copy = new List<MailMessage>();
        var ids = new HashSet<int>();
        foreach (var message in source)
        {
            if (message == null || message.id <= 0 || !ids.Add(message.id)
                || message.title == null || message.body == null || message.rewards == null
                || message.sentAt < 0 || message.expiresAt > 253402300799L || message.expiresAt <= message.sentAt
                || message.rewards.Any(item => item == null || item.itemId <= 0 || item.count <= 0 || string.IsNullOrEmpty(item.icon)))
                return false;

            var itemCopy = Copy(message);
            if (receipts.Any(item => item.id == message.id))
                itemCopy.claimed = true;

            copy.Add(itemCopy);
        }

        messages.Clear();
        messages.AddRange(copy);
        initialized = true;
        selectedId = 0;

        return true;
    }

    /// <summary>Read detached, unexpired records in stable newest-first order.</summary>
    /// <param name="now">Current UTC Unix seconds.</param>
    /// <returns>A detached inbox snapshot.</returns>
    public List<MailMessage> GetMessages(long now)
    {
        return messages.Where(item => item.expiresAt > now).OrderByDescending(item => item.sentAt)
            .ThenByDescending(item => item.id).Select(Copy).ToList();
    }

    /// <summary>Read the selected, unexpired message.</summary>
    /// <param name="now">Current UTC Unix seconds.</param>
    /// <returns>A detached record or null.</returns>
    public MailMessage GetSelected(long now)
    {
        var message = messages.Find(item => item.id == selectedId && item.expiresAt > now);

        return message == null ? null : Copy(message);
    }

    /// <summary>Select a live message and mark it read.</summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="now">Current UTC Unix seconds.</param>
    /// <returns>True when the record is available.</returns>
    public bool Select(int id, long now)
    {
        var message = messages.Find(item => item.id == id && item.expiresAt > now);
        if (message == null)
            return false;

        selectedId = id;
        message.read = true;

        return true;
    }

    /// <summary>Check current eligibility for a claim.</summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="now">Current UTC Unix seconds.</param>
    /// <returns>The business result without mutating state.</returns>
    public MailResult CheckClaim(int id, long now)
    {
        var message = messages.Find(item => item.id == id);
        if (message == null)
            return MailResult.NotFound;

        if (message.expiresAt <= now)
            return MailResult.Expired;

        if (message.claimed)
            return MailResult.AlreadyClaimed;

        return message.rewards.Count == 0 ? MailResult.NoAttachment : MailResult.Success;
    }

    /// <summary>Check deletion eligibility from current owned data.</summary>
    /// <param name="id">Message identifier.</param>
    /// <returns>The business result without mutating state.</returns>
    public MailResult CheckDelete(int id)
    {
        var message = messages.Find(item => item.id == id);
        if (message == null)
            return MailResult.NotFound;

        if (!message.read)
            return MailResult.Unread;

        return !message.claimed && message.rewards.Count > 0 ? MailResult.PendingAttachments : MailResult.Success;
    }

    /// <summary>Record a local claim exactly once without granting inventory.</summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="now">Current UTC Unix seconds.</param>
    /// <returns>The validated operation result.</returns>
    public MailResult Claim(int id, long now)
    {
        var result = CheckClaim(id, now);
        if (result != MailResult.Success)
            return result;

        var message = messages.Find(item => item.id == id);
        message.read = true;
        message.claimed = true;

        receipts.Add(Copy(message));

        return MailResult.Success;
    }

    /// <summary>Delete only read records without outstanding attachments.</summary>
    /// <param name="id">Message identifier.</param>
    /// <returns>The validated operation result.</returns>
    public MailResult Delete(int id)
    {
        var result = CheckDelete(id);
        if (result != MailResult.Success)
            return result;

        var message = messages.Find(item => item.id == id);
        messages.Remove(message);
        if (selectedId == id)
            selectedId = 0;

        return MailResult.Success;
    }

    /// <summary>Claim all eligible messages using the same per-record validation.</summary>
    /// <param name="now">Current UTC Unix seconds.</param>
    /// <returns>The number of newly recorded claims.</returns>
    public int ClaimAll(long now)
    {
        int count = 0;
        foreach (var message in GetMessages(now))
        {
            if (Claim(message.id, now) == MailResult.Success)
                count++;
        }

        return count;
    }

    /// <summary>Delete eligible read messages while preserving unread and pending mail.</summary>
    /// <returns>The number of removed messages.</returns>
    public int DeleteRead()
    {
        int count = 0;
        foreach (int id in messages.Select(item => item.id).ToArray())
        {
            if (Delete(id) == MailResult.Success)
                count++;
        }

        return count;
    }

    /// <summary>Read detached local receipts for host integration or verification.</summary>
    /// <returns>Local receipts without inventory side effects.</returns>
    public List<MailMessage> GetReceipts()
    {
        return receipts.Select(Copy).ToList();
    }

    /// <summary>Copy a record and every mutable attachment.</summary>
    /// <param name="source">Validated source record.</param>
    /// <returns>A detached record.</returns>
    private static MailMessage Copy(MailMessage source)
    {
        return new MailMessage {
            id = source.id, title = source.title, body = source.body,
            sentAt = source.sentAt, expiresAt = source.expiresAt,
            read = source.read, claimed = source.claimed,
            rewards = source.rewards.Select(item => new MailReward {
                itemId = item.itemId, count = item.count, icon = item.icon
            }).ToList()
        };
    }
}

/// <summary>Detached host-supplied mail data.</summary>
public class MailMessage
{
    /// <summary>Stable positive mail identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Dynamic subject supplied by the product.</summary>
    public string title
    { get; set; }

    /// <summary>Dynamic message body supplied by the product.</summary>
    public string body
    { get; set; }

    /// <summary>UTC send time in Unix seconds.</summary>
    public long sentAt
    { get; set; }

    /// <summary>UTC expiry time in Unix seconds.</summary>
    public long expiresAt
    { get; set; }

    /// <summary>Whether the message has been read.</summary>
    public bool read
    { get; set; }

    /// <summary>Whether attachment collection has been recorded.</summary>
    public bool claimed
    { get; set; }

    /// <summary>Detached attachment data.</summary>
    public List<MailReward> rewards
    { get; set; } = new List<MailReward>();
}

/// <summary>Detached attachment description, not an inventory item.</summary>
public class MailReward
{
    /// <summary>Product item identifier.</summary>
    public int itemId
    { get; set; }

    /// <summary>Positive attachment quantity.</summary>
    public int count
    { get; set; }

    /// <summary>Product icon asset path.</summary>
    public string icon
    { get; set; }
}

/// <summary>Business results independent of display language.</summary>
public enum MailResult
{
    /// <summary>The operation completed.</summary>
    Success,

    /// <summary>The requested record is absent.</summary>
    NotFound,

    /// <summary>The record has expired.</summary>
    Expired,

    /// <summary>A claim was already recorded.</summary>
    AlreadyClaimed,

    /// <summary>The record has no attachments.</summary>
    NoAttachment,

    /// <summary>The record has not been read.</summary>
    Unread,

    /// <summary>Attachments must be claimed before deletion.</summary>
    PendingAttachments
}
