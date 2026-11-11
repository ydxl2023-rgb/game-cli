using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using Config;

/// <summary>
/// home system
/// </summary>
public class HomeSystem : RuntimeSystem
{
    /// <summary>
    /// active home system
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        InstallUI();
        LoadFight();

        return true;
    }

    /// <summary>
    /// install home UI
    /// </summary>
    private void InstallUI()
    {
        UISystem.GetSingleton().OpenWidget<UIHome>((widget) =>
        {
            XApp.GetSingleton().HideLaunch();

            if (widget)
                SetupClicks(widget);
        });
    }

    /// <summary>
    /// deactive home system
    /// </summary>
    public override void OnDetive()
    {
        System.Type[] widgets = new System.Type[] {
            typeof(UIHome)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }

    /// <summary>
    /// try to load fight
    /// </summary>
    /// <returns></returns>
    private bool LoadFight()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (!master)
            return false;

        var chaterId = master.TryGetChapter(out int index);
        if (chaterId != 0)
        {
            var prop = Excel.propChapterById(chaterId);
            if (index >= 0 && index < prop.Levels.Count)
            {
                // load fight system with chapter id and level config id
                XPluginUtility.PostEvent(new SystemActive() {
                    type = typeof(FightSystem),
                    complete = () => {
                        if (IsActive())
                        {
                            XPluginUtility.FireEvent(new StartFightEvent() {
                                chapterId = chaterId, configId = prop.Levels[index]
                            });
                        }
                    }
                });
            }
        }
        
        return false;
    }

    /// <summary>
    /// setup home widget clicks
    /// </summary>
    /// <param name="widget"></param>
    /// <returns></returns>
    private bool SetupClicks(UIHome widget)
    {
        var vm = widget.GetVM() as VMHome;
        if (!vm)
            return false;

        SetupEmailClick(vm);
        SetupBackpackClick(vm);

        return true;
    }

    /// <summary>
    /// setup email click event, when email button is clicked, fire an EmailEvent to show the email UI
    /// </summary>
    /// <param name="vm"></param>
    private void SetupEmailClick(VMHome vm) 
    {
        vm.EmailClick = (sender) => {
            XPluginUtility.FireEvent(new EmailEvent() { show = true });
            return true;
        };
    }

    /// <summary>
    /// setup backpack click event, 
    /// when backpack button is clicked, fire a BackpackEvent to show the backpack UI
    /// </summary>
    /// <param name="vm"></param>
    private void SetupBackpackClick(VMHome vm)
    {
        vm.BackpackClick = (sender) => {
            XPluginUtility.FireEvent(new BackpackEvent() { show = true });
            return true;
        };
    }
}
