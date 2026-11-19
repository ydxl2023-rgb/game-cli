using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Coordinates the local mail reference without network or inventory services.</summary>
public class MailSystem : RuntimeSystem
{
    /// <summary>Open the inbox and bind actions after the asynchronous page load.</summary>
    /// <returns>True when opening was requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UIMail>((widget) => {
            if (widget)
            {
                var vm = widget.vm;
                if (!vm)
                    return;

                var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
                if (!entity)
                    return;

                if (!entity.initialized)
                {
                    string example = widget.GetExample();
                    var messages = string.IsNullOrEmpty(example) ? new MailMessage[0]
                        : JsonConvert.DeserializeObject<MailMessage[]>(example);
                    if (messages != null && messages.Length > 0)
                    {
                        long offset = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - messages.Max(item => item.sentAt);
                        foreach (var message in messages)
                        {
                            message.sentAt += offset;
                            message.expiresAt += offset;
                        }
                    }

                    if (!entity.ReplaceInbox(messages))
                        throw new InvalidOperationException("Invalid local mail example.");
                }

                vm.CloseClick = (sender) => Close();
                vm.MessageClick = (sender, index) => Select(index);
                vm.ClaimClick = (sender) => Claim();
                vm.DeleteClick = (sender) => Delete();
                vm.ClaimAllClick = (sender) => ClaimAll();
                vm.DeleteReadClick = (sender) => DeleteRead();
                vm.MessageTemplate = false;
                vm.RewardTemplate = false;

                Refresh();
            }
        });

        return true;
    }

    /// <summary>Release the page through the shared system-level close API.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UIMail)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: false);
    }

    /// <summary>Update list and detail snapshots from current entity data.</summary>
    /// <returns>True when the active view was updated.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIMail>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity)
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var messages = entity.GetMessages(now);
        if (entity.GetSelected(now) == null && messages.Count > 0)
            entity.Select(messages[0].id, now);

        messages = entity.GetMessages(now);
        var selected = entity.GetSelected(now);
        vm.Messages = messages.Select(item => new VMMailEntry {
            mailId = item.id,
            Title = item.title,
            SentAt = DateTimeOffset.FromUnixTimeSeconds(item.sentAt).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
            Selected = item.id == entity.selectedId,
            Unselected = item.id != entity.selectedId && !(item.claimed || item.read && item.rewards.Count == 0),
            Received = item.id != entity.selectedId && (item.claimed || item.read && item.rewards.Count == 0),
            Unread = !item.read,
            HasAttachment = item.rewards.Count > 0,
            Claimed = item.claimed,
            Pending = !item.claimed && item.rewards.Count > 0
        }).ToList();
        vm.Total = messages.Count.ToString(CultureInfo.InvariantCulture);
        vm.Unread = messages.Count(item => !item.read).ToString(CultureInfo.InvariantCulture);
        vm.EmptyVisible = messages.Count == 0;
        vm.EnvelopeState = messages.Count > 0 ? "EnvelopeOpen" : "EnvelopeClose";
        vm.DetailVisible = selected != null;
        vm.CanClaimAll = messages.Any(item => entity.CheckClaim(item.id, now) == MailResult.Success);
        vm.CanDeleteRead = messages.Any(item => entity.CheckDelete(item.id) == MailResult.Success);
        vm.Title = selected == null ? string.Empty : selected.title;
        vm.Body = selected == null ? string.Empty : selected.body;
        vm.SentAt = selected == null ? string.Empty : DateTimeOffset.FromUnixTimeSeconds(selected.sentAt).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        vm.Expiry = selected == null ? string.Empty : Math.Ceiling((selected.expiresAt - now) / 86400d).ToString(CultureInfo.InvariantCulture);
        vm.ClaimVisible = selected != null && entity.CheckClaim(selected.id, now) == MailResult.Success;
        vm.DeleteVisible = selected != null && entity.CheckDelete(selected.id) == MailResult.Success;
        vm.Rewards = (selected == null ? new System.Collections.Generic.List<MailReward>() : selected.rewards)
            .Select(item => new VMRewardEntry {
                Item = new VMItemSlot {
                    Text = item.count.ToString(CultureInfo.InvariantCulture),
                    Icon = item.icon,
                    IconVisible = true
                },
                Claimed = selected.claimed
            }).ToList();

        return true;
    }

    /// <summary>Open a displayed record by its stable identifier.</summary>
    /// <param name="index">Clicked view index.</param>
    /// <returns>True when the record was selected and rendered.</returns>
    public bool Select(int index)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIMail>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity)
            return false;

        if (vm.Messages == null || index < 0 || index >= vm.Messages.Count)
            return false;

        return entity.Select(vm.Messages[index].mailId, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) && Refresh();
    }

    /// <summary>Record the selected attachment claim and refresh the current state.</summary>
    /// <returns>True when a new receipt was recorded.</returns>
    public bool Claim()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity || !IsActive())
            return false;

        var result = entity.Claim(entity.selectedId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Refresh();

        return result == MailResult.Success;
    }

    /// <summary>Delete the selected eligible record.</summary>
    /// <returns>True when the record was deleted.</returns>
    public bool Delete()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity || !IsActive())
            return false;

        var result = entity.Delete(entity.selectedId);
        Refresh();

        return result == MailResult.Success;
    }

    /// <summary>Record all available local claims without granting items.</summary>
    /// <returns>True when at least one new receipt was recorded.</returns>
    public bool ClaimAll()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity || !IsActive())
            return false;

        int count = entity.ClaimAll(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Refresh();

        return count > 0;
    }

    /// <summary>Delete only read records with no outstanding attachments.</summary>
    /// <returns>True when at least one record was deleted.</returns>
    public bool DeleteRead()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity || !IsActive())
            return false;

        int count = entity.DeleteRead();
        Refresh();

        return count > 0;
    }

    /// <summary>Request closure through the lifecycle coordinator.</summary>
    /// <returns>True when the close request was dispatched.</returns>
    private bool Close()
    {
        return XPluginUtility.PostEvent(new SystemDetive { type = typeof(MailSystem) });
    }
}
