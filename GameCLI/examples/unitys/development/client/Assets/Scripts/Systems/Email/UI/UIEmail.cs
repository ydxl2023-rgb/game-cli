using Config;
using Oathx.Core.Sdk;
using Oathx.Widget;

[Config("Assets/Data/Widgets/Email/UIEmail.prefab", WidgetLayer.center)]
public class UIEmail : UIWidget<VMEmail>
{
    /// <summary>Identity of the message currently displayed.</summary>
    private int? displayedId;

    /// <summary>
    /// email widget animator clips
    /// </summary>
    public enum Clip
    {
        idle,
        open,
        close
    }

    /// <summary>Bind the mail service intents once when the inbox opens.</summary>
    protected override bool Bind()
    {
        vm.Close = Send<ReqEmailClose>;
        vm.ReceiveAll = Send<ReqEmailClaimAll>;
        vm.Read = sender => displayedId.HasValue && Send(new ReqEmailClaim { emailId = displayedId.Value });
        vm.FancyClick = (sender, index) => vm.Emails != null && index >= 0 && index < vm.Emails.Count &&
            Send(new ReqEmailSelectMessage { emailId = vm.Emails[index].Id });

        return true;
    }

    /// <summary>Render the current list and selected message.</summary>
    public bool Invalidate(EmailEntity entity)
    {
        if (!entity || !vm)
            return false;

        var slots = new VList<VMEmailSlot>();
        foreach (var email in entity.Emails)
            slots.Add(new VMEmailSlot { Name = email.title, Status = email.claimStatus == 0, Id = email.id });

        vm.Emails = slots;
        vm.ShowReplay = false;
        vm.ShowRead = false;
        var selected = entity.GetSelectEmail();
        if (selected != null)
            Select(selected);
        else
        {
            displayedId = null;
            vm.Content = string.Empty;
            vm.Title = string.Empty;
            vm.Rewards = new VList<VMItemSlot>();
        }

        return true;
    }

    /// <summary>
    /// select email
    /// </summary>
    /// <param name="email"></param>
    public void Select(Email email)
    {
        vm.Content = email.content;
        vm.Title = email.title;
        if (displayedId != email.id)
            vm.Clip = Clip.open.ToString();

        displayedId = email.id;

        if (!string.IsNullOrEmpty(email.reward))
        {
            SetReward(email.reward);
        }
        else
            vm.Rewards = new VList<VMItemSlot>();

        vm.ShowRead = email.claimStatus == 0;
        vm.ShowReplay = false;

        foreach (VMEmailSlot slot in vm.Emails)
        {
            slot.Select = slot.Id != email.id;
        }    
    }

    /// <summary>
    /// set email reward info
    /// </summary>
    /// <param name="reward"></param>
    public void SetReward(string reward)
    {
        var ids = reward.Split(',');

        VList<VMItemSlot> slots = new VList<VMItemSlot>();
        foreach (var id in ids)
        {
            if (int.TryParse(id, out var itemId))
            {
                var item = Excel.propItemById(itemId);
                slots.Add(new VMItemSlot() {
                    Text = item.Name
                });
            }
        }

        vm.Rewards = slots;
    }
}

/// <summary>Dismiss the open inbox.</summary>
public sealed class ReqEmailClose : UIRequestEvent
{
}

/// <summary>Select an inbox message.</summary>
public sealed class ReqEmailSelectMessage : UIRequestEvent
{
    /// <summary>Service message identifier.</summary>
    public int emailId
    { get; set; }
}

/// <summary>Claim the displayed attachment.</summary>
public sealed class ReqEmailClaim : UIRequestEvent
{
    /// <summary>Service message identifier.</summary>
    public int emailId
    { get; set; }
}

/// <summary>Claim available attachments.</summary>
public sealed class ReqEmailClaimAll : UIRequestEvent
{
}
