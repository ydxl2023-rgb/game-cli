using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using System;

/// <summary>
/// seven day login event, used to trigger showing or hiding the seven day login widget
/// </summary>
public class SevenDayLoginEvent : IEvent
{
    /// <summary>
    /// flag to indicate whether to show the seven day login widget
    /// </summary>
    public bool show { get; set; }
}

/// <summary>
/// seven day login system, responsible for managing the seven day login feature, including tracking login days,
/// handling rewards and interacting with the seven day login widget
/// </summary>
public class SevenDayLoginSystem : RuntimeSystem
{
    /// <summary>
    /// cached entity reference
    /// </summary>
    public SevenDayLoginEntity entity
    { get; private set; }

    /// <summary>
    /// flag to indicate if the system is destroyed, used to prevent processing events after detive
    /// </summary>
    private bool isDestroyed = false;

    /// <summary>
    /// install seven day login system, loads the seven day login entity for the player using their player id (PID) obtained from the master entity,
    /// </summary>
    /// <returns></returns>
    public override bool Install()
    {
        Subscribe<SevenDayLoginEvent>(OnSevenDayLoginEvent);
        Subscribe<LoginSucceed>(OnLoginSucceedEvent);

        return true;
    }

    /// <summary>
    /// uninstall seven day login system, unsubscribe all events to prevent memory leaks and unintended behavior after detive
    /// </summary>
    public override void Uninstall()
    {
        // unsubscribe all events to prevent memory leaks and unintended behavior after detive
        UnsubscribeAll();
    }

    /// <summary>
    /// on active, load entity data and subscribe events
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (!master)
            return false;

        var pid = master.GetAttr(MasterPropertyDef.PID);
        Rpc.Load<SevenDayLoginEntity>(pid, (code, sevenDayLogin) => {
            entity = sevenDayLogin;
        });

        return true;
    }

    /// <summary>
    /// on detive, save entity data, unsubscribe events and close widget
    /// </summary>
    public override void OnDetive()
    {
        isDestroyed = true;

        // force save entity data on detive
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (master && entity)
        {
            Rpc.Save(master.GetAttr(MasterPropertyDef.PID), entity);
        }

        // close widget if it's still open
        System.Type[] widgets = new System.Type[] {
            typeof(UISevenDayLogin)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }

    /// <summary>
    /// handle seven day login event, open or close widget based on event data
    /// </summary>
    /// <param name="evt"></param>
    private void OnSevenDayLoginEvent(SevenDayLoginEvent evt)
    {
        if (isDestroyed)
            return;

        if (evt.show)
        {
            UISystem.GetSingleton().OpenWidget<UISevenDayLogin>((widget) => {
                Refresh(widget);
            });
        }
        else
        {
            System.Type[] widgets = new System.Type[] {
                typeof(UISevenDayLogin)
            };

            UISystem.GetSingleton().CloseWidget(widgets);
        }
    }

    /// <summary>
    /// handle login succeed event, 
    /// check daily login and show widget if needed, this is a placeholder function and should be implemented with actual login success handling logic
    /// </summary>
    /// <param name="evt"></param>
    private void OnLoginSucceedEvent(LoginSucceed evt)
    {
        // check daily login on active to update login days and show widget if needed
        bool check = CheckDailyLogin();
        if (check)
        {
            UISystem.GetSingleton().OpenWidget<UISevenDayLogin>((widget) => {
                Refresh(widget);
            });
        }
    }

    /// <summary>
    /// check daily login, update consecutive and total days, and save data if login is valid
    /// </summary>
    private bool CheckDailyLogin()
    {
        if (!entity)
            return false;

        int offsetDay = CalcOffsetDay() + 1;
        if (offsetDay <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// calculate the number of days between account creation and current login, 
    /// this is a placeholder function and should be implemented with actual date calculation logic
    /// </summary>
    /// <param name="tmcreate"></param>
    /// <param name="loginAt"></param>
    /// <returns></returns>
    private int CalcOffsetDay()
    {
        var loginEntity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (loginEntity)
        {
            // account create time should be greater than 0,
            // otherwise it's invalid data and should not update login days
            if (loginEntity.tmcreate > 0)
            {
                var offsetCreateUtc = DateTimeOffset.FromUnixTimeMilliseconds(loginEntity.tmcreate);
                var loginAtUtcOffet = DateTimeOffset.FromUnixTimeMilliseconds(loginEntity.loginAt);

                return (loginAtUtcOffet.UtcDateTime - offsetCreateUtc.UtcDateTime).Days;
            }
        }

        // calculate the difference in days between account creation and current login
        return 0;
    }

    /// <summary>
    /// load data into widget and setup click handlers
    /// </summary>
    /// <param name="widget"></param>
    private void Refresh(UISevenDayLogin widget)
    {
        if (entity)
            widget.Invalidate(entity);
        
        SetupClicks(widget);
    }

    /// <summary>
    /// setup click handlers for fancy click, claim click and close click in the widget
    /// </summary>
    /// <param name="widget"></param>
    /// <returns></returns>
    private bool SetupClicks(UISevenDayLogin widget)
    {
        var vm = widget.GetVM() as VMSevenDayLogin;
        if (!vm)
            return false;

        SetupFancyClick(vm);
        SetupClaimClick(vm);
        SetupCloseClick(vm);

        return true;
    }

    /// <summary>
    /// setup fancy click handler to update selected day and refresh widget selection
    /// </summary>
    /// <param name="vm"></param>
    private void SetupFancyClick(VMSevenDayLogin vm)
    {
        vm.FancyClick = (view, index) =>
        {
            return true;
        };
    }

    /// <summary>
    /// setup claim click handler to claim reward for selected day, give reward, save data and refresh widget on success
    /// </summary>
    /// <param name="vm"></param>
    private void SetupClaimClick(VMSevenDayLogin vm)
    {
        vm.Claim = (sender) =>
        {
            if (!entity)
                return false;

            return true;
        };
    }

    /// <summary>
    /// setup close click handler to close the widget
    /// </summary>
    /// <param name="vm"></param>
    private void SetupCloseClick(VMSevenDayLogin vm)
    {
        vm.Close = (sender) =>
        {
            System.Type[] widgets = new System.Type[] {
                typeof(UISevenDayLogin)
            };

            UISystem.GetSingleton().CloseWidget(widgets);

            return true;
        };
    }

    /// <summary>
    /// give reward for the claimed day, this is a placeholder function and should be implemented with actual reward logic
    /// </summary>
    /// <param name="day"></param>
    private void GiveReward(int day)
    {
        // TODO: implement reward giving logic
        // Example: read reward config and add items to backpack
        // var rewardConfig = Excel.propSevenDayLoginById(day);
        // if (rewardConfig != null)
        // {
        //     foreach (var item in rewardConfig.items)
        //     {
        //         BackpackSystem.AddItem(item.itemId, item.count);
        //     }
        // }
    }
}
