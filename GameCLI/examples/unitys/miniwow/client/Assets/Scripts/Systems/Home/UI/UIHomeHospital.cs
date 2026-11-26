using System.Collections;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Requests navigation to the hospital interaction point.</summary>
public sealed class ReqHomeHospitalVisit : UIRequestEvent { }

/// <summary>Requests one production batch from the displayed material.</summary>
public sealed class ReqHomeHospitalCraft : UIRequestEvent { }

/// <summary>Requests a return from the hospital to the village.</summary>
public sealed class ReqHomeHospitalClose : UIRequestEvent { }

/// <summary>Requests the tracked task reward without performing its business action.</summary>
public sealed class ReqHomeHospitalClaim : UIRequestEvent { }

/// <summary>Presents the first hospital service using the original production interaction.</summary>
[Config("Assets/Data/Widgets/Home/UIHomeHospital.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIHomeHospital : UIWidget<VMHomeHospital>
{
    /// <summary>Authored material button disabled during its production presentation.</summary>
    [SerializeField]
    private Button materialButton;

    /// <summary>Authored flying material presentation.</summary>
    [SerializeField]
    private RectTransform flyingMaterial;

    /// <summary>Authored priest production point, separate from the patient's position.</summary>
    [SerializeField]
    private RectTransform productionTarget;

    /// <summary>Authored patient display hidden when no hero is in treatment.</summary>
    [SerializeField]
    private GameObject patient;

    /// <summary>Current presentation operation, released when this page closes.</summary>
    private Coroutine production;

    /// <summary>Releases page-owned animation before unbinding.</summary>
    public override void Deactive()
    {
        if (production != null)
            StopCoroutine(production);

        production = null;

        base.Deactive();
    }

    /// <summary>Projects committed inventory, service attributes and the current task.</summary>
    /// <param name="village">Authoritative village state.</param>
    /// <param name="task">Current tracked task.</param>
    public void Invalidate(HomeVillageEntity village, TaskEntity task)
    {
        var service = village.service;
        int levelNumber = village.buildings.Level(service.buildingId);
        var level = Config.Excel.homeBuildingLevelConfig[service.buildingId * 1000 + levelNumber];
        var item = Config.Excel.homeBuildingItemConfig[service.productId];
        var material = Config.Excel.homeBuildingItemConfig[service.materialId];
        vm.Title = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_LEVEL), levelNumber);
        vm.Bonus = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_BONUS),
            level.PricePercent, level.CutSeconds, level.Capacity);
        vm.ProductName = CommonText.Get(item.NameTextID);
        vm.ProductIcon = item.IconPath;
        vm.MaterialIcon = material.IconPath;
        vm.ProductStock = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_STOCK),
            village.Count(service.productId));
        vm.MaterialStock = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_MATERIAL),
            village.Count(service.materialId));
        vm.Detail = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_DETAIL),
            service.recoveryPercent, service.seconds, service.price);
        vm.Gold = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_GOLD),
            village.Count(village.configuration.GoldID));
        vm.Revenue = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_REVENUE),
            village.Snapshot().hospitalRevenue);
        vm.TaskText = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_TASK),
            Config.Excel.propLanguageById(int.Parse(task.configuration.Description)));
        vm.ClaimVisible = task.status == TaskStatus.Completed;
        ShowTreatment(village);
    }

    /// <summary>Updates the actual patient without resetting input or production.</summary>
    /// <param name="village">Current recovery data.</param>
    public void ShowTreatment(HomeVillageEntity village)
    {
        patient.SetActive(village.recovery == HeroRecoveryPhase.Healing);
        if (village.recovery == HeroRecoveryPhase.Healing)
            vm.Treatment = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_TREATING),
                village.hero.healthPercent, village.remainingSeconds);
        else
            vm.Treatment = CommonText.Get(village.recovery == HeroRecoveryPhase.Traveling
                ? Config.Excel.TEXT_HOSPITAL_TRAVELING : village.recovery == HeroRecoveryPhase.Complete
                    ? Config.Excel.TEXT_HOSPITAL_FINISHED : Config.Excel.TEXT_HOSPITAL_IDLE);
    }

    /// <summary>Displays a real transaction rejection.</summary>
    public void ShowMissingMaterial()
    {
        vm.Feedback = CommonText.Get(Config.Excel.TEXT_HOSPITAL_NO_MATERIAL);
    }

    /// <summary>Starts the original post-success production presentation.</summary>
    /// <param name="buildingId">Hospital presentation configuration identity.</param>
    public void PlayProduction(int buildingId)
    {
        if (production != null)
            StopCoroutine(production);

        production = StartCoroutine(AnimateProduction(Config.Excel.homeHospitalSettingConfig[buildingId]));
    }

    /// <summary>Binds named input intentions and localized labels.</summary>
    /// <returns>Whether all intentions are bound.</returns>
    protected override bool Bind()
    {
        vm.CraftClick = Send<ReqHomeHospitalCraft>;
        vm.CloseClick = Send<ReqHomeHospitalClose>;
        vm.ClaimClick = Send<ReqHomeHospitalClaim>;
        flyingMaterial.gameObject.SetActive(false);

        return true;
    }

    /// <summary>Animates the material independently of the already committed recipe.</summary>
    /// <param name="setting">Original presentation durations.</param>
    /// <returns>The page-owned presentation operation.</returns>
    private IEnumerator AnimateProduction(Config.HomeHospitalSetting setting)
    {
        materialButton.interactable = false;
        flyingMaterial.gameObject.SetActive(true);
        Vector3 start = materialButton.transform.position;
        Vector3 target = productionTarget.position;
        float elapsed = 0f;
        while (elapsed < setting.CraftSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            flyingMaterial.position = Vector3.Lerp(start, target, elapsed / setting.FlightSeconds);
            flyingMaterial.gameObject.SetActive(elapsed < setting.FlightSeconds);
            vm.Feedback = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_CRAFTING),
                Mathf.Max(0f, setting.CraftSeconds - elapsed));

            yield return null;
        }

        vm.Feedback = string.Empty;
        materialButton.interactable = true;
        production = null;
    }
}
