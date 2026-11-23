using System;
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

        UnsubscribeAll();

        Subscribe<ReqMailClose>(request => Close());
        Subscribe<ReqMailClaim>(request => Claim());
        Subscribe<ReqMailDelete>(request => Delete());
        Subscribe<ReqMailClaimAll>(request => ClaimAll());
        Subscribe<ReqMailDeleteRead>(request => DeleteRead());
        Subscribe<ReqMailSelectMessage>(request => SelectMessage(request.mailId));

        UISystem.GetSingleton().OpenWidget<UIMail>((widget) => {
            if (widget)
            {
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

        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        if (!entity)
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var messages = entity.GetMessages(now);
        if (entity.GetSelected(now) == null && messages.Count > 0)
            entity.Select(messages[0].id, now);

        return widget.Invalidate(entity);
    }

    /// <summary>Open a displayed record by its stable identifier.</summary>
    /// <param name="index">Clicked view index.</param>
    /// <returns>True when the record was selected and rendered.</returns>
    public bool Select(int index)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIMail>(out var widget))
            return false;

        return widget.TryGetMailId(index, out var id) && SelectMessage(id);
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

    /// <summary>Select the record identified when the player clicked the displayed row.</summary>
    private bool SelectMessage(int id)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<MailEntity>();

        return IsActive() && entity && entity.Select(id, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) && Refresh();
    }

    /// <summary>Request closure through the lifecycle coordinator.</summary>
    /// <returns>True when the close request was dispatched.</returns>
    private bool Close()
    {
        return XPluginUtility.PostEvent(new SystemDetive { type = typeof(MailSystem) });
    }
}
