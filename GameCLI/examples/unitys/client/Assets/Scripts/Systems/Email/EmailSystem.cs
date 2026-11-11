using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Core.Sdk;
using Oathx.Entity;
using Oathx.HttpProtocol;
using Oathx.Widget;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class EmailEvent : IEvent
{
    /// <summary>
    /// show or hide email widget
    /// </summary>
    public bool show
    { get; set; }
}

/// <summary>
/// 
/// </summary>
public class EmailClaimEvent : IEvent
{
    /// <summary>
    /// 
    /// </summary>
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
    /// install email system
    /// </summary>
    /// <returns></returns>
    public override bool Install()
    {
        Subscribe<EmailEvent>(OnEmailEvent);
        return true;
    }

    /// <summary>
    /// uninstall email system
    /// </summary>
    public override void Uninstall()
    {
        UnsubscribeAll();
    }

    /// <summary>
    /// active email system
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (!master)
            return false;

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
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (master)
        {
            var pid = master.GetAttr(MasterPropertyDef.PID);
            Rpc.Save(pid, entity);
        }

        System.Type[] widgets = new System.Type[] {
            typeof(UIEmail)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
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
                FetchEmails(widget);
            });
        }
        else
        {
            System.Type[] widgets = new System.Type[] {
                typeof(UIEmail)
            };

            UISystem.GetSingleton().CloseWidget(widgets);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="widget"></param>
    /// <returns></returns>
    private bool SetupClicks(UIEmail widget)
    {
        var vm = widget.GetVM() as VMEmail;
        if (!vm)
            return false;

        SetupFancyClick(vm);
        SetupReceiveAllClick(vm);
        SetupCloseClick(vm);
        SetupReadClick(vm);

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="widget"></param>
    private void SetupFancyClick(VMEmail vm)
    {
        vm.FancyClick = (view, index) =>
        {
            if (!entity)
                return false;
                
            var email = entity.GetEmail(index);
            if (email != null)
            {
                entity.UpdateSelect(index);

                var widget = UISystem.GetSingleton().GetWidget<UIEmail>();
                if (widget)
                    widget.Select(email);
            }
                
            return true;
        };   
    }

    /// <summary>
    /// click  receive all
    /// </summary>
    /// <param name="widget"></param>
    private void SetupReceiveAllClick(VMEmail vm)
    {
        vm.ReceiveAll = (sender) =>
        {
            if (entity)
            {
                var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();

                var emails = entity.Emails;
                foreach(var email in emails )
                {
                    if (email.claimStatus == 0)
                    {
                        XAppEmail.GetSingleton().Claim(master.GetAttr(MasterPropertyDef.PID), email.id, (success, text) =>
                        {
                            if (success)
                            {
                                Claim(email);
                            }
                        });
                    }
                }
            }

            return true;
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="widget"></param>
    private void SetupCloseClick(VMEmail vm)
    {
        vm.Close = (sender) => {
            System.Type[] widgets = new System.Type[] {
                typeof(UIEmail)
            };

            UISystem.GetSingleton().CloseWidget(widgets);

            return true;
        };    
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vm"></param>
    private void SetupReadClick(VMEmail vm)
    {
        vm.Read = (sender) => {
            var emailEntity = XEntityManager.GetSingleton().QueryEntity<EmailEntity>();
            if (emailEntity)
            {
                var email = emailEntity.GetSelectEmail();
                if (email != null && email.claimStatus == 0)
                {
                    var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
                    if (!master)
                        return false;

                    XAppEmail.GetSingleton().Claim(master.GetAttr(MasterPropertyDef.PID), email.id, (success, text) => {
                        if (success)
                        {
                            Claim(email);
                        }
                    });
                }
            }

            return true;
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    private void Claim(Email email)
    {
        if (TryParseEmailRewards(email, out var items))
        {
            XPluginUtility.FireEvent(new GrabItemEvent() {
                items = items
            });
        }

        var widget = UISystem.GetSingleton().GetWidget<UIEmail>();
        if (widget)
        {
            widget.ClaimSucceed(email);
        }

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
                            bool succeed = emailEntity.SetEmails(result.data);
                            if (succeed)
                                Rpc.Save(pid, emailEntity);
                        }
                            
                        widget.Full(emailEntity);
                    }
                }
            });
        }

        SetupClicks(widget);
    }
}
