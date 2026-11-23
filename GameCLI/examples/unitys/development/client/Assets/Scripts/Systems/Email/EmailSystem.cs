using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Core.Sdk;
using Oathx.Entity;
using Oathx.HttpProtocol;
using Oathx.Widget;
using System.Collections.Generic;

/// <summary>
/// Request an inbox visibility change.
/// </summary>
public class EmailEvent : IEvent
{
    /// <summary>
    /// show or hide email widget
    /// </summary>
    public bool show
    { get; set; }
}

/// <summary>Mail reward delivery notification.</summary>
public class EmailClaimEvent : IEvent
{
    /// <summary>Items granted by the mail service.</summary>
    public List<ItemEntity> items
    { get; set; } = new List<ItemEntity>();
}

/// <summary>
/// 
/// </summary>
public class EmailSystem : RuntimeSystem
{
    /// <summary>
    /// email entity
    /// </summary>
    public EmailEntity entity 
    { get; private set; }

    /// <summary>
    /// active email system
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (!master)
            return false;

        UnsubscribeAll();

        Subscribe<EmailEvent>(OnEmailEvent);
        Subscribe<ReqEmailClose>(request => CloseInbox());
        Subscribe<ReqEmailSelectMessage>(request => SelectEmail(request.emailId));
        Subscribe<ReqEmailClaim>(request => ClaimEmail(request.emailId));
        Subscribe<ReqEmailClaimAll>(request => ClaimAll());

        Rpc.Load<EmailEntity>(master.GetAttr(MasterPropertyDef.PID), (code, email) => {
            if (code == HttpCode.SUCCEED)
            {
                entity = email;
            }
        });

        return true;
    }

    /// <summary>
    /// deactive email system
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (master)
        {
            var pid = master.GetAttr(MasterPropertyDef.PID);
            Rpc.Save(pid, entity);
        }

        CloseInbox();
    }

    /// <summary>
    /// show email
    /// </summary>
    /// <param name="evt"></param>
    private void OnEmailEvent(EmailEvent evt)
    {
        if (evt.show)
        {
            UISystem.GetSingleton().OpenWidget<UIEmail>((widget) => {
                if (widget)
                    FetchEmails(widget);
            });
        }
        else
        {
            CloseInbox();
        }
    }

    /// <summary>Close the inbox through the system-owned window list.</summary>
    private void CloseInbox()
    {
        System.Type[] widgets = new System.Type[] {
            typeof(UIEmail)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }

    /// <summary>Display the selected message by its service identifier.</summary>
    private void SelectEmail(int emailId)
    {
        if (entity && entity.SelectEmail(emailId))
            RefreshInbox();
    }

    /// <summary>Claim the requested attachment through the existing mail service.</summary>
    private void ClaimEmail(int emailId)
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (entity && master)
        {
            var email = entity.Emails.Find(item => item.id == emailId);
            if (email != null && email.claimStatus == 0)
            {
                XAppEmail.GetSingleton().Claim(master.GetAttr(MasterPropertyDef.PID), email.id, (success, text) => {
                    if (success)
                        Claim(email);
                });
            }
        }
    }

    /// <summary>Request each outstanding attachment.</summary>
    private void ClaimAll()
    {
        if (entity)
        {
            foreach (var email in entity.Emails)
            {
                if (email.claimStatus == 0)
                    ClaimEmail(email.id);
            }
        }
    }

    /// <summary>Refresh the open page after the inbox changes.</summary>
    private void RefreshInbox()
    {
        if (UISystem.GetSingleton().TryGetWidget<UIEmail>(out var widget))
            widget.Invalidate(entity);
    }

    /// <summary>Deliver the granted rewards after service confirmation.</summary>
    private void Claim(Email email)
    {
        email.claimStatus = 1;

        if (TryParseEmailRewards(email, out var items))
        {
            XPluginUtility.FireEvent(new GrabItemEvent() {
                items = items
            });
        }

        RefreshInbox();

        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (entity && master)
            Rpc.Save(master.GetAttr(MasterPropertyDef.PID), entity);
    }

    /// <summary>
    /// parse email raward items
    /// </summary>
    /// <param name="email"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    private bool TryParseEmailRewards(Email email, out List<ItemEntity> items)
    {
        items = new List<ItemEntity>();
        if (!string.IsNullOrEmpty(email.reward))
        {
            var ids = email.reward.Split(',');
            foreach (var id in ids)
            {
                if (int.TryParse(id, out var itemId))
                {
                    items.Add(new ItemEntity(itemId));
                }
            }
        }

        return true;
    }

    /// <summary>
    /// fetch remote emails
    /// </summary>
    private void FetchEmails(UIEmail widget)
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (master)
        {
            var pid = master.GetAttr(MasterPropertyDef.PID);
            XAppEmail.GetSingleton().GetEmails(pid, (success, text) => {
                if (success && widget)
                {
                    var result = JsonConvert.DeserializeObject<EmailResult>(text);
                    if (result != null && result.data != null)
                    {
                        var emailEntity = XEntityManager.GetSingleton().QueryEntity<EmailEntity>();
                        if (emailEntity)
                        {
                            entity = emailEntity;
                            bool succeed = emailEntity.SetEmails(result.data);
                            if (succeed)
                                Rpc.Save(pid, emailEntity);
                        }
                            
                        widget.Invalidate(emailEntity);
                    }
                }
            });
        }

    }
}
