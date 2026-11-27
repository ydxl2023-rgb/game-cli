using UnityEngine;

/// <summary>Displays authored fire effects and the current objective in the Home scene.</summary>
public sealed class HomeOpeningScene : MonoBehaviour
{
    /// <summary>Original burned auction model displayed before rescue.</summary>
    [SerializeField]
    private GameObject auctionDamaged;

    /// <summary>Original restored auction model displayed after rescue.</summary>
    [SerializeField]
    private GameObject auctionRestored;

    /// <summary>Entity-bound merchant presentation at the original scene anchor.</summary>
    [SerializeField]
    private ActorShape merchant;

    /// <summary>Original hotel models before and after extinguishing its fire.</summary>
    [SerializeField]
    private GameObject[] hotelModels;

    /// <summary>Original hospital models before and after the second rescue.</summary>
    [SerializeField]
    private GameObject[] hospitalModels;

    /// <summary>Original constructed hospital model, distinct from the extinguished ruins.</summary>
    [SerializeField]
    private GameObject hospitalBuilt;

    /// <summary>Original constructed inn model selected by its persisted building level.</summary>
    [SerializeField]
    private GameObject hotelBuilt;

    /// <summary>Original constructed forge model.</summary>
    [SerializeField]
    private GameObject blacksmithBuilt;

    /// <summary>Original unrepaired forge model.</summary>
    [SerializeField]
    private GameObject blacksmithInitial;

    /// <summary>Entity-bound Garen shape reused instead of duplicating the prototype visitor.</summary>
    [SerializeField]
    private ActorShape garen;

    /// <summary>Existing entity-bound hotel owner.</summary>
    [SerializeField]
    private ActorShape hotelOwner;

    /// <summary>Fire presentation for the hotel and hospital.</summary>
    [SerializeField]
    private GameObject[] secondFires;

    /// <summary>Original people-bubble anchors indexed by configured rescue ID minus one.</summary>
    [SerializeField]
    private Transform[] rescueTargets;

    /// <summary>Current authored effect target selected during invalidation.</summary>
    private Transform rescueTarget;

    /// <summary>Native smoke presentation using the recovered source texture.</summary>
    [SerializeField]
    private ParticleSystem clearingSmoke;

    /// <summary>Original well presentation before water collection.</summary>
    [SerializeField]
    private GameObject wellAvailable;

    /// <summary>Original well presentation while the player carries water.</summary>
    [SerializeField]
    private GameObject wellCollected;

    /// <summary>Native auction fire presentation using the recovered sprite sheet.</summary>
    [SerializeField]
    private GameObject auctionFire;

    /// <summary>Water stream shown only during the actual rescue operation.</summary>
    [SerializeField]
    private LineRenderer waterStream;

    /// <summary>World-space destination marker.</summary>
    [SerializeField]
    private Transform destination;

    /// <summary>Shows the active stream between the player and the auction fire.</summary>
    /// <param name="active">Whether the rescue is executing.</param>
    /// <param name="origin">The authoritative player position.</param>
    public void ShowRescue(bool active, Vector3 origin)
    {
        waterStream.gameObject.SetActive(active);
        if (!active)
            return;

        Vector3 start = origin + Vector3.up;
        Vector3 end = rescueTarget.position;
        for (int index = 0; index < waterStream.positionCount; index++)
        {
            float fraction = index / (float)(waterStream.positionCount - 1);
            Vector3 point = Vector3.Lerp(start, end, fraction);
            point.y += Mathf.Sin(fraction * Mathf.PI) * 1.2f;
            waterStream.SetPosition(index, point);
        }
    }

    /// <summary>Starts or clears the owned dust without accumulating particles after cancellation.</summary>
    /// <param name="active">Whether the debris operation is executing.</param>
    public void ShowClearing(bool active)
    {
        if (active && !clearingSmoke.isPlaying)
            clearingSmoke.Play();
        else if (!active && !clearingSmoke.isStopped)
            clearingSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    /// <summary>Updates presentation from authoritative opening data.</summary>
    /// <param name="opening">The current rescue data.</param>
    public void Invalidate(HomeOpening opening)
    {
        bool restored = opening.auctionRestored;
        auctionDamaged.SetActive(!restored);
        auctionRestored.SetActive(restored);
        merchant.gameObject.SetActive(restored);
        auctionFire.SetActive(!restored);
        hotelModels[0].SetActive(!opening.hotelRescued);
        hotelModels[1].SetActive(opening.hotelRescued);
        for (int index = 0; index < hospitalModels.Length; index++)
            hospitalModels[index].SetActive(index == opening.hospitalStage);
        hotelOwner.gameObject.SetActive(opening.hotelRescued);
        secondFires[0].SetActive(!opening.hotelRescued);
        secondFires[1].SetActive(!opening.hospitalRescued);
        rescueTarget = rescueTargets[opening.rescue.ID - 1];
        bool collected = opening.water > 0 || opening.hospitalRescued;
        wellAvailable.SetActive(!collected);
        wellCollected.SetActive(collected);
        bool water = opening.fetchingWater;
        destination.gameObject.SetActive(water || opening.canOperate);
        destination.position = water ? opening.GetWaterPoint() : opening.GetRescuePoint();
    }

    /// <summary>Applies business construction after the opening presentation is updated.</summary>
    /// <param name="opening">Current opening phase.</param>
    /// <param name="village">Actual ownership and construction data.</param>
    public void ShowVillage(HomeOpening opening, HomeVillageEntity village)
    {
        bool forgeBuilt = village.buildings.Level(60002) > 0;
        blacksmithBuilt.SetActive(forgeBuilt);
        blacksmithInitial.SetActive(!forgeBuilt);

        bool innBuilt = village.buildings.Level(60017) > 0;
        hotelBuilt.SetActive(innBuilt);
        if (innBuilt)
        {
            foreach (var model in hotelModels)
                model.SetActive(false);
        }

        bool built = village.buildings.Level(village.configuration.BuildingID) > 0;
        hospitalBuilt.SetActive(built);
        if (built)
        {
            foreach (var model in hospitalModels)
                model.SetActive(false);
        }

        garen.gameObject.SetActive(opening.phase == HomeOpeningPhase.AwaitRecruit
            && village.recovery != HeroRecoveryPhase.Healing && !village.dining);
    }
}
