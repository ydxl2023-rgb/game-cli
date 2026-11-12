using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Coordinates local login and requests other systems through Master.</summary>
public class LoginSystem : RuntimeSystem
{
    /// <summary>Open the login page asynchronously.</summary>
    /// <returns>True when an open request was started.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UILogin>((widget) => {
            if (widget)
            {
                var vm = widget.vm;
                if (!vm)
                    return;

                var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
                if (!entity)
                    return;

                vm.EnterClick = (sender) => EnterGame();
                vm.AnnouncementClick = (sender) => OpenNotice();
                vm.ChangeClick = (sender) => ReportPending();
                vm.FriendClick = (sender) => ReportPending();
                vm.UserCenterClick = (sender) => ReportPending();
                vm.CancellationClick = (sender) => ReportPending();
                vm.ClearClick = (sender) => ReportPending();
                vm.LegalClick = (sender) => ReportPending();
                vm.NoticeClick = (sender) => ReportPending();
                vm.AgeIconBtnClick = (sender) => ReportPending();
                vm.ExitClick = (sender) => ReportPending();
                vm.CanEnter = true;
                vm.Friends = true;
                vm.ServerName = CommonText.Get(Config.Excel.LOGIN_SERVER_DEFAULT);
                vm.VersionText = Application.version;
                widget.Invalidate(entity);

                XApp.GetSingleton().ShowLaunchBackground();
            }
        });

        return true;
    }

    /// <summary>Release subscriptions and close the page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UILogin)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Publish the explicitly local login result.</summary>
    /// <returns>True when the result was queued.</returns>
    private bool EnterGame()
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        return XPluginUtility.PostEvent(new LoginSucceed { pid = entity.pid });
    }

    /// <summary>Request the notice module.</summary>
    /// <returns>True when the request was queued.</returns>
    private bool OpenNotice()
    {
        if (!IsActive())
            return false;

        return XPluginUtility.PostEvent(new SystemActive { type = typeof(NoticeSystem) });
    }

    /// <summary>Display the pending integration message without inventing business behavior.</summary>
    /// <returns>False because the requested integration is pending.</returns>
    private bool ReportPending()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        vm.ServerStatus = CommonText.Get(Config.Excel.LOGIN_INTEGRATION_PENDING);

        return false;
    }
}

