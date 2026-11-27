using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Oathx.Widget;

/// <summary>Requests closing the inn.</summary>
public sealed class ReqInnClose : UIRequestEvent { }

/// <summary>Requests the first inn construction transaction.</summary>
public sealed class ReqInnRepair : UIRequestEvent { }

/// <summary>Requests one configured food batch.</summary>
public sealed class ReqInnCraft : UIRequestEvent { }

/// <summary>Requests a meal for the owned hero.</summary>
public sealed class ReqInnRest : UIRequestEvent { }

/// <summary>Requests claiming the currently completed inn task.</summary>
public sealed class ReqInnClaim : UIRequestEvent { }

/// <summary>Projects inn state and sends named business intentions.</summary>
[Config("Assets/Data/Widgets/Inn/UIInn.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIInn : UIWidget<VMInn>
{
    /// <summary>Authored material control disabled during the production presentation.</summary>
    [SerializeField]
    private Button materialButton;

    /// <summary>Authored flight sprite.</summary>
    [SerializeField]
    private RectTransform flyingMaterial;

    /// <summary>Authored innkeeper production point.</summary>
    [SerializeField]
    private RectTransform productionTarget;

    /// <summary>Owned production animation.</summary>
    private Coroutine production;

    /// <summary>Releases the animation with the page.</summary>
    public override void Deactive()
    {
        if (production != null)
            StopCoroutine(production);

        production = null;

        base.Deactive();
    }

    /// <summary>Projects committed inventory and hero service state.</summary>
    /// <param name="inn">Inn rule identity.</param>
    /// <param name="home">Current Home aggregate.</param>
    /// <param name="task">Current tracked task.</param>
    public void Invalidate(InnEntity inn, HomeEntity home, TaskEntity task)
    {
        var village = home.village;
        var state = village.Snapshot();
        var service = village.buildings.GetService(inn.buildingId);
        int levelNumber = village.buildings.Level(inn.buildingId);
        var level = Config.Excel.homeBuildingLevelConfig[inn.buildingId * 1000 + Math.Max(1, levelNumber)];
        var product = Config.Excel.homeBuildingItemConfig[service.productId];
        var material = Config.Excel.homeBuildingItemConfig[service.materialId];
        vm.Title = string.Format(CommonText.Get(Config.Excel.TEXT_INN_LEVEL), levelNumber);
        vm.Gold = village.Count(village.configuration.GoldID).ToString();
        vm.Gem = home.Snapshot().profile.gem.ToString();
        vm.Revenue = state.innRevenue.ToString();
        vm.RepairVisible = levelNumber == 0;
        vm.BusinessVisible = levelNumber > 0;
        vm.RestVisible = levelNumber > 0;
        vm.Patient = state.innRestEndTicks != 0;
        bool relevant = task != null && task.configuration.TaskBuildingType == Config.ETaskBuildingType.Hotel;
        vm.TaskText = relevant
            ? Config.Excel.propLanguageById(int.Parse(task.configuration.Description)) : string.Empty;
        vm.ClaimVisible = relevant && task.status == TaskStatus.Completed;
        vm.RepairCost = string.Format(CommonText.Get(Config.Excel.TEXT_INN_REPAIR),
            village.buildings.GetRepairCosts(inn.buildingId).Sum(row => row.Amount));
        vm.ProductName = CommonText.Get(product.NameTextID);
        vm.ProductIcon = product.IconPath;
        vm.MaterialIcon = material.IconPath;
        vm.ProductStock = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_STOCK),
            village.Count(service.productId));
        vm.MaterialStock = village.Count(service.materialId).ToString();
        vm.ProductsVisible = village.Count(service.productId) > 0;
        vm.EmptyVisible = !vm.ProductsVisible;
        vm.PriceBonus = "+" + level.PricePercent + "%";
        vm.TimeBonus = "-" + level.CutSeconds + "s";
        vm.CapacityBonus = level.Capacity.ToString();
        vm.Detail = string.Format(CommonText.Get(Config.Excel.TEXT_INN_DETAIL),
            service.recoveryPercent, service.seconds, service.price);
        vm.Energy = state.innRestEndTicks == 0
            ? string.Format(CommonText.Get(Config.Excel.TEXT_INN_ENERGY), state.energyPercent)
            : string.Format(CommonText.Get(Config.Excel.TEXT_INN_RESTING),
                Math.Max(0, (state.innRestEndTicks - DateTime.UtcNow.Ticks) / (double)TimeSpan.TicksPerSecond));
    }

    /// <summary>Displays a localized outcome without changing business state.</summary>
    /// <param name="textId">Common language identity.</param>
    public void ShowFeedback(int textId)
    {
        vm.Feedback = CommonText.Get(textId);
    }

    /// <summary>Plays the original duration material flight after a committed recipe.</summary>
    public void PlayProduction()
    {
        production = StartCoroutine(AnimateProduction());
    }

    /// <summary>Binds named UI intentions.</summary>
    /// <returns>Whether bindings were installed.</returns>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqInnClose>;
        vm.RepairClick = Send<ReqInnRepair>;
        vm.CraftClick = Send<ReqInnCraft>;
        vm.RestClick = Send<ReqInnRest>;
        vm.ClaimClick = Send<ReqInnClaim>;
        flyingMaterial.gameObject.SetActive(false);

        return true;
    }

    /// <summary>Shows material flight and production without owning economic state.</summary>
    /// <returns>The page-owned presentation.</returns>
    private IEnumerator AnimateProduction()
    {
        materialButton.interactable = false;
        float elapsed = 0f;
        float seconds = Config.Excel.homeInnSettingConfig[60017].CraftSeconds;
        Vector3 start = materialButton.transform.position;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            flyingMaterial.gameObject.SetActive(elapsed < 0.5f);
            flyingMaterial.position = Vector3.Lerp(start, productionTarget.position, elapsed / 0.5f);
            vm.Feedback = string.Format(CommonText.Get(Config.Excel.TEXT_HOSPITAL_CRAFTING),
                Mathf.Max(0f, seconds - elapsed));

            yield return null;
        }

        vm.Feedback = CommonText.Get(Config.Excel.TEXT_INN_CRAFTED);
        materialButton.interactable = true;
        production = null;
    }
}
