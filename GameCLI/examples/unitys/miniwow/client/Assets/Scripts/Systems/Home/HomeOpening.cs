using Config;
using Oathx.Core;
using UnityEngine;

/// <summary>Durable steps of the opening village rescues.</summary>
public enum HomeOpeningPhase
{
    /// <summary>The original opening movie is playing.</summary>
    Movie,
    /// <summary>The priest explains the rescue.</summary>
    Introduction,
    /// <summary>The player must enter the original well region.</summary>
    FetchWater,
    /// <summary>The player carries water to the auction house.</summary>
    CarryWater,
    /// <summary>The merchant thanks the player after the fire is extinguished.</summary>
    Thanks,
    /// <summary>The completed task awaits an explicit claim.</summary>
    Claim,
    /// <summary>Legacy saved boundary migrated after the first task's claim receipt is read.</summary>
    NextTask,
    /// <summary>The second task requests a fresh bucket of water.</summary>
    FetchSecondWater,
    /// <summary>The player carries ten water to the hotel.</summary>
    CarryHotelWater,
    /// <summary>The hotel owner speaks after consuming five water.</summary>
    HotelThanks,
    /// <summary>The remaining five water must reach the hospital.</summary>
    CarryHospitalWater,
    /// <summary>Both second-task fires are extinguished and await claim.</summary>
    SecondClaim,
    /// <summary>The hospital beam must be cleared before the trapped person can be saved.</summary>
    ClearDebris,
    /// <summary>The beam is cleared and the person is ready for rescue.</summary>
    RescuePerson,
    /// <summary>The saved person awaits the original automatic task claim.</summary>
    ThirdComplete,
    /// <summary>Task 60004 is available for the next recruitment feature.</summary>
    AwaitRecruit
}

/// <summary>Reports a change in the entity-owned opening progress.</summary>
public sealed class HomeOpeningChanged : IEvent { }

/// <summary>Owns opening rules and durable progress without scene or UI references.</summary>
public sealed class HomeOpening
{
    /// <summary>Storage key derived from the owning entity member.</summary>
    private static readonly string storageKey = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.opening);

    /// <summary>The selected opening configuration row.</summary>
    public Config.HomeOpening configuration
    { get; private set; }

    /// <summary>The last completed durable transition.</summary>
    public HomeOpeningPhase phase
    { get; private set; }

    /// <summary>Water carried for the current rescue.</summary>
    public int water
    { get; private set; }

    /// <summary>Whether the saved rescue has restored the auction house.</summary>
    public bool auctionRestored
    { get { return phase >= HomeOpeningPhase.Thanks; } }

    /// <summary>Whether the player can collect a bucket in the current task.</summary>
    public bool fetchingWater
    { get { return phase == HomeOpeningPhase.FetchWater || phase == HomeOpeningPhase.FetchSecondWater; } }

    /// <summary>Whether a rescue destination currently accepts carried water.</summary>
    public bool carryingWater
    {
        get
        {
            return phase == HomeOpeningPhase.CarryWater || phase == HomeOpeningPhase.CarryHotelWater
                || phase == HomeOpeningPhase.CarryHospitalWater;
        }
    }

    /// <summary>Whether the active step accepts an operation at its destination.</summary>
    public bool canOperate
    {
        get
        {
            return carryingWater || phase == HomeOpeningPhase.ClearDebris || phase == HomeOpeningPhase.RescuePerson;
        }
    }

    /// <summary>The original hospital model stage reached by durable gameplay.</summary>
    public int hospitalStage
    {
        get
        {
            if (phase >= HomeOpeningPhase.AwaitRecruit)
                return 3;

            if (phase >= HomeOpeningPhase.RescuePerson)
                return 2;

            return hospitalRescued ? 1 : 0;
        }
    }

    /// <summary>Whether a completed rescue task is awaiting its claim.</summary>
    public bool canClaim
    { get { return phase == HomeOpeningPhase.Claim || phase == HomeOpeningPhase.SecondClaim; } }

    /// <summary>Whether the current task has a durable completion observation.</summary>
    public bool taskCompleted
    { get { return phase == HomeOpeningPhase.Thanks || canClaim || phase == HomeOpeningPhase.ThirdComplete; } }

    /// <summary>Whether the hotel rescue has completed, independently of task claim.</summary>
    public bool hotelRescued
    { get { return phase >= HomeOpeningPhase.HotelThanks; } }

    /// <summary>Whether the hospital has reached the original second damaged model.</summary>
    public bool hospitalRescued
    { get { return phase >= HomeOpeningPhase.SecondClaim; } }

    /// <summary>The configured destination and cost for the current rescue.</summary>
    public Config.OpeningRescue rescue
    {
        get
        {
            int id;
            if (phase >= HomeOpeningPhase.RescuePerson)
                id = 5;
            else if (phase == HomeOpeningPhase.ClearDebris)
                id = 4;
            else if (phase >= HomeOpeningPhase.CarryHospitalWater)
                id = 3;
            else
                id = phase >= HomeOpeningPhase.FetchSecondWater ? 2 : 1;

            return Excel.openingRescueConfig[id];
        }
    }

    /// <summary>Loads the selected definition and saved opening step.</summary>
    /// <param name="definition">The exported opening configuration.</param>
    public HomeOpening(Config.HomeOpening definition)
    {
        configuration = definition;
        phase = (HomeOpeningPhase)PlayerPrefs.GetInt(storageKey, 0);
        water = carryingWater ? definition.WaterAmount : 0;
        if (phase == HomeOpeningPhase.HotelThanks || phase == HomeOpeningPhase.CarryHospitalWater)
            water = definition.WaterAmount - Excel.openingRescueConfig[2].WaterCost;
    }

    /// <summary>Advances only user-confirmed dialogue or movie boundaries.</summary>
    /// <returns>Whether the requested transition was accepted.</returns>
    public bool Continue()
    {
        switch (phase)
        {
            case HomeOpeningPhase.Movie:
                SetPhase(HomeOpeningPhase.Introduction);
                return true;
            case HomeOpeningPhase.Introduction:
                SetPhase(HomeOpeningPhase.FetchWater);
                return true;
            case HomeOpeningPhase.Thanks:
                SetPhase(HomeOpeningPhase.Claim);
                return true;
            case HomeOpeningPhase.HotelThanks:
                SetPhase(HomeOpeningPhase.CarryHospitalWater);
                return true;
            default:
                return false;
        }
    }

    /// <summary>Collects water once when the player reaches the original source volume.</summary>
    /// <param name="position">The authoritative actor position.</param>
    /// <returns>Whether water was collected.</returns>
    public bool CollectWater(Vector3 position)
    {
        var center = new Vector3(configuration.WaterX, configuration.WaterY, configuration.WaterZ);
        var size = new Vector3(configuration.WaterSizeX, configuration.WaterSizeY, configuration.WaterSizeZ);
        if (!fetchingWater || !new Bounds(center, size).Contains(position))
            return false;

        water = configuration.WaterAmount;
        SetPhase(phase == HomeOpeningPhase.FetchWater
            ? HomeOpeningPhase.CarryWater : HomeOpeningPhase.CarryHotelWater);

        return true;
    }

    /// <summary>Checks the actual arrival position and water cost before rescue.</summary>
    /// <param name="position">The authoritative actor position.</param>
    /// <returns>Whether the rescue can execute at this position.</returns>
    public bool CanOperate(Vector3 position)
    {
        Vector3 offset = position - GetRescuePoint();
        offset.y = 0f;

        return canOperate
            && water >= rescue.WaterCost
            && offset.magnitude <= configuration.InteractionRadius;
    }

    /// <summary>Commits the completed operation and its configured cost after arrival and duration checks.</summary>
    /// <param name="position">The final actor position.</param>
    /// <returns>Whether the fire was extinguished.</returns>
    public bool CompleteOperation(Vector3 position)
    {
        if (!CanOperate(position))
            return false;

        water -= rescue.WaterCost;
        switch (phase)
        {
            case HomeOpeningPhase.CarryWater:
                SetPhase(HomeOpeningPhase.Thanks);
                break;
            case HomeOpeningPhase.CarryHotelWater:
                SetPhase(HomeOpeningPhase.HotelThanks);
                break;
            case HomeOpeningPhase.CarryHospitalWater:
                SetPhase(HomeOpeningPhase.SecondClaim);
                break;
            case HomeOpeningPhase.ClearDebris:
                SetPhase(HomeOpeningPhase.RescuePerson);
                break;
            case HomeOpeningPhase.RescuePerson:
                SetPhase(HomeOpeningPhase.ThirdComplete);
                break;
        }

        return true;
    }

    /// <summary>Advances only after the task collection commits the corresponding claim.</summary>
    /// <param name="tasks">The authoritative task collection.</param>
    public void Reconcile(TaskCollectionEntity tasks)
    {
        if (tasks.GetTask(configuration.TaskID).status == TaskStatus.Claimed && phase <= HomeOpeningPhase.NextTask)
            SetPhase(HomeOpeningPhase.FetchSecondWater);

        if (tasks.GetTask(configuration.NextTaskID).status == TaskStatus.Claimed
            && phase < HomeOpeningPhase.ClearDebris)
            SetPhase(HomeOpeningPhase.ClearDebris);

        var third = Excel.openingRescueConfig[4];
        if (tasks.GetTask(third.TaskID).status == TaskStatus.Claimed && phase < HomeOpeningPhase.AwaitRecruit)
            SetPhase(HomeOpeningPhase.AwaitRecruit);
    }

    /// <summary>Returns the original destination for the current building.</summary>
    /// <returns>The configured world-space target.</returns>
    public Vector3 GetRescuePoint()
    {
        var target = rescue;

        return new Vector3(target.TargetX, target.TargetY, target.TargetZ);
    }

    /// <summary>Returns the original guide point for this task's water collection.</summary>
    /// <returns>The configured world-space well approach point.</returns>
    public Vector3 GetWaterPoint()
    {
        var target = rescue;

        return new Vector3(target.WaterTargetX, target.WaterTargetY, target.WaterTargetZ);
    }

    /// <summary>Persists a completed boundary before notifying presentation.</summary>
    /// <param name="value">The accepted next phase.</param>
    private void SetPhase(HomeOpeningPhase value)
    {
        phase = value;
        PlayerPrefs.SetInt(storageKey, (int)phase);
        PlayerPrefs.Save();
    }
}
