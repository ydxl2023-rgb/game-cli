using System.Collections;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Commits construction after its cost-flight presentation.</summary>
public sealed class ReqHomeHospitalRepair : UIRequestEvent { }

/// <summary>Requests the interface belonging to a restored resident.</summary>
public sealed class ReqHomeResidentVisit : UIRequestEvent
{
    /// <summary>Original building configuration identity.</summary>
    public int buildingId
    { get; set; }
}

/// <summary>Projects the original repair or enter bubble onto its authored scene anchor.</summary>
[Config("Assets/Data/Widgets/Home/UIHomeHospitalEntry.prefab", WidgetLayer.background, cacheWidget: false)]
public sealed class UIHomeHospitalEntry : UIWidget<VMHomeHospitalEntry>
{
    /// <summary>Authored bubble root positioned in the page coordinate system.</summary>
    [SerializeField]
    private RectTransform bubble;

    /// <summary>Independent original NPC title projection.</summary>
    [SerializeField]
    private RectTransform buildingTitle;

    /// <summary>Resident titles ordered by the shared scene binding manifest.</summary>
    [SerializeField]
    private RectTransform[] residentTitles;

    /// <summary>Original merchant entrance button projected independently of its title.</summary>
    [SerializeField]
    private RectTransform merchantEntry;

    /// <summary>Original forge entrance projected onto its scene mount.</summary>
    [SerializeField]
    private RectTransform forgeEntry;

    /// <summary>Original hotel entrance projected onto its scene mount.</summary>
    [SerializeField]
    private RectTransform hotelEntry;

    /// <summary>Authored currency flight display.</summary>
    [SerializeField]
    private RectTransform coin;

    /// <summary>Authored interaction button.</summary>
    [SerializeField]
    private Button entryButton;

    /// <summary>Scene projection adapter supplied by the Home owner.</summary>
    private HomeScene scene;

    /// <summary>Whether the latest entity state permits showing this entry.</summary>
    private bool available;

    /// <summary>Whether construction has committed.</summary>
    private bool built;

    /// <summary>Owned cost-flight presentation.</summary>
    private Coroutine flight;

    /// <summary>Cancels the uncommitted presentation when Home closes.</summary>
    public override void Deactive()
    {
        if (flight != null)
            StopCoroutine(flight);

        flight = null;
        if (scene)
            scene.ShowHospitalSelection(false);

        scene = null;

        base.Deactive();
    }

    /// <summary>Receives the existing scene projection without searching its hierarchy.</summary>
    /// <param name="context">Scene owned by HomeRuntime.</param>
    public void BindScene(HomeScene context)
    {
        scene = context;
    }

    /// <summary>Displays the actual construction state and configured cost.</summary>
    /// <param name="village">Authoritative village state.</param>
    /// <param name="tasks">Feature prerequisite data.</param>
    public void Invalidate(HomeVillageEntity village, TaskCollectionEntity tasks)
    {
        int id = village.configuration.BuildingID;
        available = village.buildings.IsAvailable(id, tasks);
        built = village.buildings.Level(id) > 0;
        vm.BuildingName = CommonText.Get(Config.Excel.homeBuildingConfig[id].NpcNameTextID);
        vm.BuiltVisible = built;
        vm.RepairVisible = !built;
        vm.Cost = built ? string.Empty : village.buildings.GetRepairCosts(id)[0].Amount.ToString();
    }

    /// <summary>Shows a failed affordability check without changing the building.</summary>
    public void ShowMissingGold()
    {
        vm.Feedback = CommonText.Get(Config.Excel.TEXT_HOSPITAL_NO_GOLD);
    }

    /// <summary>Plays the cost flight once before requesting the transaction.</summary>
    /// <param name="buildingId">Hospital presentation setting identity.</param>
    public void PlayRepair(int buildingId)
    {
        if (flight == null)
            flight = StartCoroutine(FlyCost(Config.Excel.homeHospitalSettingConfig[buildingId].FlightSeconds));
    }

    /// <summary>Binds the world entry intention.</summary>
    /// <returns>Whether the binding completed.</returns>
    protected override bool Bind()
    {
        vm.EnterClick = Send<ReqHomeHospitalVisit>;
        vm.RepairClick = Send<ReqHomeHospitalVisit>;
        vm.MerchantClick = OnMerchantClick;
        vm.ForgeClick = OnForgeClick;
        vm.HotelClick = OnHotelClick;
        coin.gameObject.SetActive(false);

        return true;
    }

    /// <summary>Tracks the original world anchor while the camera follows the player.</summary>
    private void LateUpdate()
    {
        if (!scene)
            return;

        Vector3 screen = scene.GetHospitalScreenPoint();
        bool nearby = scene.IsPlayerAtHospital();
        bool visible = available && scene.AcceptsInput() && screen.z > 0f;
        vm.EntryVisible = visible && (!built || nearby);
        scene.ShowHospitalSelection(visible && nearby);
        buildingTitle.gameObject.SetActive(visible && built);
        buildingTitle.anchoredPosition = ToLocal(scene.GetHospitalTitleScreenPoint());
        RefreshResidentTitles();
        RefreshResidentEntry(merchantEntry, 60019);
        RefreshResidentEntry(forgeEntry, 60002);
        RefreshResidentEntry(hotelEntry, 60017);
        if (!scene.AcceptsInput() && flight != null)
        {
            StopCoroutine(flight);
            flight = null;
            coin.gameObject.SetActive(false);
            entryButton.interactable = true;
        }

        bubble.anchoredPosition = ToLocal(screen);
    }

    /// <summary>Tracks resident titles independently of hospital availability and proximity.</summary>
    private void RefreshResidentTitles()
    {
        for (int index = 0; index < residentTitles.Length; index++)
        {
            Vector3 screen = scene.GetNpcTitleScreenPoint(index);
            var title = residentTitles[index];
            title.gameObject.SetActive(scene.IsNpcTitleVisible(index) && screen.z > 0f);
            title.anchoredPosition = ToLocal(screen);
        }
    }

    /// <summary>Requests the merchant interface without executing a purchase.</summary>
    /// <param name="button">The clicked entrance.</param>
    /// <returns>Whether the intention was sent.</returns>
    private bool OnMerchantClick(Button button)
    {
        return Send(new ReqHomeResidentVisit { buildingId = 60019 });
    }

    /// <summary>Requests the forge interface.</summary>
    /// <param name="button">The clicked entrance.</param>
    /// <returns>Whether the intention was sent.</returns>
    private bool OnForgeClick(Button button)
    {
        return Send(new ReqHomeResidentVisit { buildingId = 60002 });
    }

    /// <summary>Requests the hotel interface.</summary>
    /// <param name="button">The clicked entrance.</param>
    /// <returns>Whether the intention was sent.</returns>
    private bool OnHotelClick(Button button)
    {
        return Send(new ReqHomeResidentVisit { buildingId = 60017 });
    }

    /// <summary>Projects a resident entry using the shared original proximity rule.</summary>
    /// <param name="entry">Authored entrance presentation.</param>
    /// <param name="buildingId">Original building configuration identity.</param>
    private void RefreshResidentEntry(RectTransform entry, int buildingId)
    {
        Vector3 screen = scene.GetNpcEntryScreenPoint(buildingId);
        entry.gameObject.SetActive(scene.IsNpcEntryVisible(buildingId) && screen.z > 0f);
        entry.anchoredPosition = ToLocal(screen);
    }

    /// <summary>Converts a screen point into the full-stretch root's local coordinates.</summary>
    /// <param name="screen">Projected screen coordinates.</param>
    /// <returns>The root-relative position.</returns>
    private Vector2 ToLocal(Vector3 screen)
    {
        var root = (RectTransform)transform;

        return new Vector2((screen.x / Screen.width - 0.5f) * root.rect.width,
            (screen.y / Screen.height - 0.5f) * root.rect.height);
    }

    /// <summary>Moves the currency from the logical player to the repair bubble.</summary>
    /// <param name="seconds">Configured presentation duration.</param>
    /// <returns>The widget-owned flight operation.</returns>
    private IEnumerator FlyCost(float seconds)
    {
        entryButton.interactable = false;
        vm.Feedback = string.Empty;
        coin.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            coin.anchoredPosition = Vector2.Lerp(ToLocal(scene.GetPlayerScreenPoint()),
                bubble.anchoredPosition, elapsed / seconds);

            yield return null;
        }

        coin.gameObject.SetActive(false);
        entryButton.interactable = true;
        flight = null;
        Send(new ReqHomeHospitalRepair());
    }
}
