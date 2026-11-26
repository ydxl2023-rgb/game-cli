using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Owns hospital interaction, production presentation and treatment scheduling.</summary>
public sealed class HospitalSystem : RuntimeSystem
{
    /// <summary>Owned treatment clock, stopped with this system.</summary>
    private Coroutine treatment;

    /// <summary>Subscribes hospital business intentions independently of Home.</summary>
    /// <returns>Whether subscriptions were installed.</returns>
    public override bool OnActive()
    {
        Subscribe<ReqHomeHospitalVisit>(OnHospitalVisit);
        Subscribe<HomeHospitalArrived>(OnHospitalArrived);
        Subscribe<ReqHomeHospitalRepair>(OnHospitalRepair);
        Subscribe<ReqHomeHospitalCraft>(OnHospitalCraft);
        Subscribe<ReqHomeHospitalClose>(OnHospitalClose);
        Subscribe<ReqHomeHospitalClaim>(OnHospitalClaim);
        Subscribe<HomeVillageChanged>(OnVillageChanged);
        Subscribe<TaskChanged>(OnTaskChanged);
        Subscribe<HomeReady>(OnHomeReady);
        Subscribe<HomeSessionAvailable>(OnSessionAvailable);
        Subscribe<HomeSessionClosing>(OnSessionClosing);
        Subscribe<HospitalPatientArrived>(OnPatientArrived);

        XPluginUtility.FireEvent(new HomeSessionRequested());

        return true;
    }

    /// <summary>Releases owned pages and treatment scheduling.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();
        CloseHospital();
    }

    /// <summary>Releases treatment and presentation when this system or its Home session closes.</summary>
    private void CloseHospital()
    {
        if (treatment != null)
            XCoroutine.StopCoroutine(treatment);

        treatment = null;
        System.Type[] widgets = new System.Type[] { typeof(UIHomeHospital) };
        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10006, visible = false });
    }

    /// <summary>Starts treatment scheduling when the session's scene is ready.</summary>
    /// <param name="message">The initialized Home session.</param>
    private void OnHomeReady(HomeReady message)
    {
        StartTreatment();
    }

    /// <summary>Attaches a reinstalled hospital to the existing Home session.</summary>
    /// <param name="message">The current Home availability response.</param>
    private void OnSessionAvailable(HomeSessionAvailable message)
    {
        StartTreatment();
    }

    /// <summary>Releases session references before Home disposes its scene.</summary>
    /// <param name="message">The closing Home session.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        CloseHospital();
    }

    /// <summary>Replaces the owned clock when attaching to a ready session.</summary>
    private void StartTreatment()
    {
        if (treatment != null)
            XCoroutine.StopCoroutine(treatment);

        treatment = XCoroutine.Run(UpdateTreatment());
    }

    /// <summary>Refreshes the hospital page after shared inventory or recovery changes.</summary>
    /// <param name="message">Committed shared state.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        RefreshHospital();
    }

    /// <summary>Refreshes the task panel after progression or reward changes.</summary>
    /// <param name="message">Committed task state.</param>
    private void OnTaskChanged(TaskChanged message)
    {
        RefreshHospital();
    }

    /// <summary>Consumes medicine only after scene navigation reports actual patient arrival.</summary>
    /// <param name="message">The patient arrival observation.</param>
    private void OnPatientArrived(HospitalPatientArrived message)
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (home.village.BeginHealing())
            XPluginUtility.FireEvent(new HomeVillageChanged());
    }

    /// <summary>Projects shared committed data into the owned page.</summary>
    private void RefreshHospital()
    {
        var ui = UISystem.GetSingleton();
        if (ui.TryGetWidget<UIHomeHospital>(out var hospital))
        {
            var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
            var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
            hospital.Invalidate(home.village, tasks.GetTrackedTask(), home.Snapshot().profile.gem);
        }
    }

    /// <summary>Schedules recovery rules while scene navigation remains owned by Home.</summary>
    /// <returns>The system-owned treatment operation.</returns>
    private IEnumerator UpdateTreatment()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var village = home.village;
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        while (true)
        {
            var task = tasks.GetTrackedTask();
            if (village.recovery == HeroRecoveryPhase.Idle
                && Config.Excel.villageTaskConfig.TryGetValue(task.configuration.ID, out var step)
                && (HomeVillageAction)step.Action == HomeVillageAction.Heal
                && village.Execute(step.ID, tasks))
                XPluginUtility.FireEvent(new HomeVillageChanged());

            if (village.recovery == HeroRecoveryPhase.Healing)
            {
                if (village.CompleteHealing())
                    XPluginUtility.FireEvent(new HomeVillageChanged());
                else
                {
                    RefreshHospital();
                    XPluginUtility.FireEvent(new HomeVillageTreatmentProgress());
                }
            }

            yield return null;
        }
    }

    /// <summary>Starts navigation only when the hospital feature is available.</summary>
    /// <param name="message">Hospital interaction intention.</param>
    private void OnHospitalVisit(ReqHomeHospitalVisit message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!entity.village.buildings.IsAvailable(entity.village.configuration.BuildingID, tasks))
            return;

        XPluginUtility.FireEvent(new HospitalNavigationRequested());
    }

    /// <summary>Chooses construction or the shop after actual player arrival.</summary>
    /// <param name="message">Navigation completion.</param>
    private void OnHospitalArrived(HomeHospitalArrived message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var village = entity.village;
        int id = village.configuration.BuildingID;
        var ui = UISystem.GetSingleton();
        if (village.buildings.Level(id) > 0)
        {
            XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10006, visible = true });
            ui.OpenWidget<UIHomeHospital>(OnHospitalOpened);

            return;
        }

        bool affordable = true;
        foreach (var cost in village.buildings.GetRepairCosts(id))
            affordable &= village.Count(cost.ItemID) >= cost.Amount;

        XPluginUtility.FireEvent(new HospitalRepairPresentationRequested {
            buildingId = id,
            affordable = affordable
        });
    }

    /// <summary>Commits construction after presentation while rechecking current state.</summary>
    /// <param name="message">Completed cost-flight intention.</param>
    private void OnHospitalRepair(ReqHomeHospitalRepair message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (entity.village.Execute(HomeVillageAction.Repair, tasks))
            XPluginUtility.FireEvent(new HomeVillageChanged());
    }

    /// <summary>Initializes the production page from actual village data.</summary>
    /// <param name="widget">Initialized hospital page.</param>
    private void OnHospitalOpened(UIHomeHospital widget)
    {
        if (widget)
        {
            var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
            var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
            widget.Invalidate(entity.village, tasks.GetTrackedTask(), entity.Snapshot().profile.gem);
        }
    }

    /// <summary>Commits a recipe before playing the original success animation.</summary>
    /// <param name="message">Material click intention.</param>
    private void OnHospitalCraft(ReqHomeHospitalCraft message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!UISystem.GetSingleton().TryGetWidget<UIHomeHospital>(out var widget))
            return;

        if (!entity.village.Execute(HomeVillageAction.Craft, tasks))
        {
            widget.ShowMissingMaterial();

            return;
        }

        XPluginUtility.FireEvent(new HomeVillageChanged());
        widget.PlayProduction(entity.village.configuration.BuildingID);
    }

    /// <summary>Closes only the hospital presentation and restores current Home input.</summary>
    /// <param name="message">Return intention.</param>
    private void OnHospitalClose(ReqHomeHospitalClose message)
    {
        System.Type[] widgets = new System.Type[] { typeof(UIHomeHospital) };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10006, visible = false });
    }

    /// <summary>Claims an already completed task without triggering a business operation.</summary>
    /// <param name="message">Reward intention.</param>
    private void OnHospitalClaim(ReqHomeHospitalClaim message)
    {
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = tasks.GetTrackedTask();
        if (task.status == TaskStatus.Completed)
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = task.configuration.ID });
    }

}
