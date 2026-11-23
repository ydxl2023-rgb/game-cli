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
    /// on active, load entity data and subscribe events
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (!master)
            return false;

        isDestroyed = false;
        UnsubscribeAll();

        Subscribe<SevenDayLoginEvent>(OnSevenDayLoginEvent);
        Subscribe<LoginSucceed>(OnLoginSucceedEvent);
        Subscribe<ReqSevenDayLoginSelectDay>(OnDayRequested);
        Subscribe<ReqSevenDayLoginClaim>(OnClaimRequested);
        Subscribe<ReqSevenDayLoginClose>(OnCloseRequested);

        var pid = master.GetAttr(MasterPropertyDef.PID);
        Rpc.Load<SevenDayLoginEntity>(pid, (code, sevenDayLogin) => {
            if (IsActive() && sevenDayLogin)
            {
                entity = sevenDayLogin;
                if (UISystem.GetSingleton().TryGetWidget<UISevenDayLogin>(out var widget))
                    widget.Invalidate(entity);
            }
        });

        return true;
    }

    /// <summary>
    /// on detive, save entity data, unsubscribe events and close widget
    /// </summary>
    public override void OnDetive()
    {
        if (entity)
            entity.selectedDay = null;
        isDestroyed = true;
        UnsubscribeAll();

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

    /// <summary>Render the loaded sign-in data on the visible widget.</summary>
    private void Refresh(UISevenDayLogin widget)
    {
        if (widget && entity)
            widget.Invalidate(entity);
    }

    /// <summary>Accept a visible day selection while the sign-in module is active.</summary>
    private void OnDayRequested(ReqSevenDayLoginSelectDay request)
    {
        if (entity && entity.days.Exists(item => item.day == request.day))
            entity.selectedDay = request.day;
    }

    /// <summary>Delegate an eligible day to the existing reward integration hook.</summary>
    private void OnClaimRequested(ReqSevenDayLoginClaim request)
    {
        if (entity && entity.selectedDay.HasValue)
            GiveReward(entity.selectedDay.Value);
    }

    /// <summary>Close the sign-in page after an explicit player request.</summary>
    private void OnCloseRequested(ReqSevenDayLoginClose request)
    {
        if (entity)
            entity.selectedDay = null;

        System.Type[] widgets = new System.Type[] {
            typeof(UISevenDayLogin)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }

    /// <summary>Product reward integration point.</summary>
    private void GiveReward(int day)
    {
        // Reward delivery is supplied by the product.
    }
}
