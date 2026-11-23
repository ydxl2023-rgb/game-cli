using System.Globalization;
using System.Linq;
using System;
using System.Collections;
using Oathx.Widget;
using UnityEngine;

/// <summary>Displays a host-owned inbox through standard Widget bindings.</summary>
[Config("Assets/Data/Widgets/Mail/Mail.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIMail : UIWidget<VMMail>
{
    /// <summary>Optional example inbox, used only before a product supplies data.</summary>
    [SerializeField]
    private TextAsset example;

    /// <summary>Original page entrance and closing states.</summary>
    [SerializeField]
    private Animator entrance;

    /// <summary>Interaction gate while the page closes.</summary>
    [SerializeField]
    private CanvasGroup interaction;

    /// <summary>Bind inbox actions and list templates to this VM.</summary>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqMailClose>;
        vm.MessageClick = (sender, index) => RequestSelection(index);
        vm.ClaimClick = Send<ReqMailClaim>;
        vm.DeleteClick = Send<ReqMailDelete>;
        vm.ClaimAllClick = Send<ReqMailClaimAll>;
        vm.DeleteReadClick = Send<ReqMailDeleteRead>;
        vm.MessageTemplate = false;
        vm.RewardTemplate = false;

        return true;
    }

    /// <summary>Restore the original entrance when activated.</summary>
    /// <returns>True when the base widget activated.</returns>
    public override bool Active()
    {
        bool active = base.Active();
        if (!active || !vm)
            return false;

        if (entrance && interaction)
        {
            interaction.interactable = true;
            entrance.Play("Open", 0, 0f);
        }

        return active;
    }

    /// <summary>Complete framework closure after the source exit animation.</summary>
    /// <param name="complete">Framework completion callback.</param>
    public override void Close(Action complete)
    {
        StartCoroutine(PlayClose(complete));
    }

    /// <summary>Read the optional local example without owning business data.</summary>
    /// <returns>The example JSON or an empty string.</returns>
    public string GetExample()
    {
        return example ? example.text : string.Empty;
    }

    /// <summary>Refresh the visible inbox and details from a single entity snapshot.</summary>
    public bool Invalidate(MailEntity entity)
    {
        if (!entity || !vm)
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var messages = entity.GetMessages(now);
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

    /// <summary>Resolve a displayed row before sending its selection request.</summary>
    public bool TryGetMailId(int index, out int id)
    {
        id = 0;
        if (!vm || vm.Messages == null || index < 0 || index >= vm.Messages.Count)
            return false;

        id = vm.Messages[index].mailId;

        return true;
    }

    /// <summary>Publish the stable identifier of the clicked message.</summary>
    private bool RequestSelection(int index)
    {
        return TryGetMailId(index, out var id) && Send(new ReqMailSelectMessage { mailId = id });
    }

    /// <summary>Play the original closing state without allowing another action.</summary>
    /// <param name="complete">Framework completion callback.</param>
    /// <returns>The source-duration close sequence.</returns>
    private IEnumerator PlayClose(Action complete)
    {
        if (interaction)
            interaction.interactable = false;

        if (entrance)
        {
            entrance.Play("Close", 0, 0f);
            yield return new WaitForSecondsRealtime(0.27f);
        }

        complete?.Invoke();
    }
}

/// <summary>Close the inbox.</summary>
public sealed class ReqMailClose : UIRequestEvent
{
}

/// <summary>Claim the selected attachment.</summary>
public sealed class ReqMailClaim : UIRequestEvent
{
}

/// <summary>Delete the selected message.</summary>
public sealed class ReqMailDelete : UIRequestEvent
{
}

/// <summary>Claim all eligible inbox attachments.</summary>
public sealed class ReqMailClaimAll : UIRequestEvent
{
}

/// <summary>Delete eligible read messages.</summary>
public sealed class ReqMailDeleteRead : UIRequestEvent
{
}

/// <summary>Select the message identified by the displayed row.</summary>
public sealed class ReqMailSelectMessage : UIRequestEvent
{
    /// <summary>Stable message identifier.</summary>
    public int mailId
    { get; set; }
}
