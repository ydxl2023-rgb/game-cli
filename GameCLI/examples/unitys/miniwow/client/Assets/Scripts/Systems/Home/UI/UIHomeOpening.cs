using Oathx.Widget;
using Oathx.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>Requests the next movie or dialogue boundary.</summary>
public sealed class ReqHomeOpeningContinue : UIRequestEvent { }

/// <summary>Requests movement to the auction and execution of its rescue.</summary>
public sealed class ReqHomeOpeningRescue : UIRequestEvent { }

/// <summary>Requests an explicit claim of the completed first task.</summary>
public sealed class ReqHomeOpeningClaim : UIRequestEvent { }

/// <summary>Requests the current village interaction or its reward confirmation.</summary>
public sealed class ReqHomeVillageAction : UIRequestEvent { }

/// <summary>Displays the original opening movie and entity-owned rescue progress.</summary>
[Config("Assets/Data/Widgets/Home/UIHomeOpening.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIHomeOpening : UIWidget<VMHomeOpening>
{
    /// <summary>Authored player and original video clip reference.</summary>
    [SerializeField]
    private VideoPlayer movie;

    /// <summary>Render target display with no dependency on the scene camera.</summary>
    [SerializeField]
    private RawImage screen;

    /// <summary>Original priest portrait.</summary>
    [SerializeField]
    private Sprite priest;

    /// <summary>Original merchant portrait.</summary>
    [SerializeField]
    private Sprite merchant;

    /// <summary>Original hotel owner portrait.</summary>
    [SerializeField]
    private Sprite hotelOwner;

    /// <summary>Recovered Garen conversation portrait.</summary>
    [SerializeField]
    private Sprite garen;

    /// <summary>Authored confirmation button disabled while the hero travels or heals.</summary>
    [SerializeField]
    private Button continueButton;

    /// <summary>Whether this overlay currently presents village interactions.</summary>
    private bool villageVisible;

    /// <summary>Water icon hidden during the later debris and person operations.</summary>
    [SerializeField]
    private GameObject waterIcon;

    /// <summary>Authored dialogue portrait display.</summary>
    [SerializeField]
    private Image portrait;

    /// <summary>Original separate opening music, stopped with the video.</summary>
    [SerializeField]
    private AudioSource music;

    /// <summary>Owned video render surface released with this widget.</summary>
    private RenderTexture videoTexture;

    /// <summary>Stops decoder callbacks before the widget closes.</summary>
    public override void Deactive()
    {
        StopMovie();

        base.Deactive();
    }

    /// <summary>Releases the video before the uncached widget is unbound and destroyed.</summary>
    /// <param name="model">The view model being released.</param>
    public override void Unref(VM model)
    {
        StopMovie();

        base.Unref(model);
    }

    /// <summary>Displays current business progress without mutating it.</summary>
    /// <param name="opening">The first rescue state.</param>
    /// <param name="task">The task selected by the task collection.</param>
    public void Invalidate(HomeOpening opening, TaskEntity task)
    {
        villageVisible = false;
        continueButton.interactable = true;
        vm.ContinueLabel = CommonText.Get(Config.Excel.TEXT_OPENING_CONTINUE);
        HomeOpeningPhase phase = opening.phase;
        vm.MovieVisible = phase == HomeOpeningPhase.Movie;
        vm.DialogueVisible = phase == HomeOpeningPhase.Introduction || phase == HomeOpeningPhase.Thanks
            || phase == HomeOpeningPhase.HotelThanks;
        vm.ObjectiveVisible = phase >= HomeOpeningPhase.FetchWater && phase < HomeOpeningPhase.AwaitRecruit;
        vm.RescueVisible = opening.canOperate || phase == HomeOpeningPhase.FetchSecondWater;
        vm.ClaimVisible = opening.canClaim;
        vm.RescueLabel = CommonText.Get(opening.fetchingWater
            ? Config.Excel.TEXT_OPENING_FETCH : opening.rescue.ActionTextID);
        waterIcon.SetActive(phase < HomeOpeningPhase.ClearDebris);
        vm.WaterCount = phase >= HomeOpeningPhase.ClearDebris ? ""
            : string.Format(CommonText.Get(Config.Excel.TEXT_OPENING_WATER_COUNT),
                opening.water, opening.configuration.WaterAmount);
        if (vm.DialogueVisible)
        {
            bool intro = phase == HomeOpeningPhase.Introduction;
            int dialogueId = intro ? opening.configuration.IntroDialogueID : opening.rescue.DialogueID;
            var dialogue = Config.Excel.openingDialogueConfig[dialogueId];
            vm.Speaker = CommonText.Get(dialogue.SpeakerTextID);
            vm.Dialogue = CommonText.Get(dialogue.ContentTextID);
            portrait.sprite = intro ? priest : phase == HomeOpeningPhase.HotelThanks ? hotelOwner : merchant;
        }

        vm.Objective = GetObjective(opening, task);
        if (phase == HomeOpeningPhase.Movie)
        {
            if (videoTexture == null)
                PlayMovie();
        }
        else
        {
            StopMovie();
        }
    }

    /// <summary>Displays a configured business interaction, inventory and actual treatment state.</summary>
    /// <param name="village">Authoritative village transactions.</param>
    /// <param name="task">Currently tracked task.</param>
    public void InvalidateVillage(HomeVillageEntity village, TaskEntity task)
    {
        villageVisible = Config.Excel.villageTaskConfig.TryGetValue(task.configuration.ID, out var step);
        if (villageVisible && (HomeVillageAction)step.Action != HomeVillageAction.Recruit)
        {
            villageVisible = false;
            vm.DialogueVisible = false;
            vm.ObjectiveVisible = false;

            return;
        }

        vm.DialogueVisible = villageVisible;
        if (!villageVisible)
            return;

        var config = village.configuration;
        var action = (HomeVillageAction)step.Action;
        vm.Speaker = CommonText.Get(step.TitleTextID);
        portrait.sprite = action == HomeVillageAction.Recruit || action == HomeVillageAction.Heal ? garen
            : action == HomeVillageAction.Purchase ? merchant : priest;
        vm.Dialogue = string.Format(CommonText.Get(step.DetailTextID),
            action == HomeVillageAction.Repair ? village.buildings.GetRepairCosts(config.BuildingID)[0].Amount
                : action == HomeVillageAction.Purchase ? config.PurchasePrice : config.MaterialCost,
            config.OutputCount);
        vm.ContinueLabel = CommonText.Get(step.ActionTextID);
        if (village.awaitingRecruitmentConfirmation)
        {
            vm.Dialogue = CommonText.Get(Config.Excel.TEXT_VILLAGE_RECEIVED);
            vm.ContinueLabel = CommonText.Get(Config.Excel.TEXT_VILLAGE_CLOSE);
        }
        else if (task.status == TaskStatus.Completed)
        {
            vm.Dialogue = CommonText.Get(Config.Excel.TEXT_VILLAGE_TASK_DONE);
            vm.ContinueLabel = CommonText.Get(Config.Excel.TEXT_VILLAGE_CLAIM);
        }

        if (action == HomeVillageAction.Heal)
        {
            vm.Dialogue += "\n" + string.Format(CommonText.Get(Config.Excel.TEXT_VILLAGE_HEALTH),
                village.hero.healthPercent, village.remainingSeconds);
            continueButton.interactable = task.status == TaskStatus.Completed;
            if (!continueButton.interactable)
                vm.ContinueLabel = CommonText.Get(village.recovery == HeroRecoveryPhase.Healing
                    ? Config.Excel.TEXT_VILLAGE_HEALING : Config.Excel.TEXT_VILLAGE_TRAVELING);
        }

        vm.WaterCount = string.Format(CommonText.Get(Config.Excel.TEXT_VILLAGE_STOCK),
            village.Count(config.GoldID), village.Count(config.MaterialID), village.Count(village.service.productId));
        vm.ObjectiveVisible = true;
        vm.Objective = vm.Speaker;
        waterIcon.SetActive(false);
        vm.RescueVisible = false;
        vm.ClaimVisible = false;
    }

    /// <summary>Updates only the active treatment countdown without rebuilding economic presentation.</summary>
    /// <param name="village">The current recovery state.</param>
    public void ShowTreatmentProgress(HomeVillageEntity village)
    {
        vm.Dialogue = CommonText.Get(Config.Excel.TEXT_VILLAGE_HEAL_DETAIL) + "\n"
            + string.Format(CommonText.Get(Config.Excel.TEXT_VILLAGE_HEALTH),
                village.hero.healthPercent, village.remainingSeconds);
    }

    /// <summary>Displays a rejected purchase or recipe without fabricating task progress.</summary>
    public void ShowVillageFailure()
    {
        vm.Dialogue = CommonText.Get(Config.Excel.TEXT_VILLAGE_REJECTED);
    }

    /// <summary>Shows operation time without rebuilding the UI or interrupting input.</summary>
    /// <param name="opening">The authoritative progress used to restore resource presentation.</param>
    /// <param name="active">Whether an operation countdown is active.</param>
    /// <param name="remaining">Remaining operation seconds.</param>
    public void ShowOperationProgress(HomeOpening opening, bool active, float remaining)
    {
        bool showWater = !active && opening.phase < HomeOpeningPhase.ClearDebris;
        waterIcon.SetActive(showWater);
        if (active)
            vm.WaterCount = string.Format(CommonText.Get(Config.Excel.TEXT_OPENING_COUNTDOWN), remaining);
        else
            vm.WaterCount = showWater
                ? string.Format(CommonText.Get(Config.Excel.TEXT_OPENING_WATER_COUNT),
                    opening.water, opening.configuration.WaterAmount) : "";
    }

    /// <summary>Binds named intentions and localized labels.</summary>
    /// <returns>True after all callbacks are assigned.</returns>
    protected override bool Bind()
    {
        vm.ContinueClick = OnContinue;
        vm.RescueClick = Send<ReqHomeOpeningRescue>;
        vm.ClaimClick = Send<ReqHomeOpeningClaim>;
        vm.SkipClick = OnSkip;
        vm.ConfirmClick = OnConfirm;
        vm.CancelClick = OnCancel;
        vm.RetryClick = OnRetry;
        vm.ContinueLabel = CommonText.Get(Config.Excel.TEXT_OPENING_CONTINUE);
        vm.SkipLabel = CommonText.Get(Config.Excel.TEXT_OPENING_SKIP);
        vm.ConfirmLabel = CommonText.Get(Config.Excel.TEXT_OPENING_SKIP);
        vm.CancelLabel = CommonText.Get(Config.Excel.TEXT_OPENING_CANCEL);
        vm.ConfirmText = CommonText.Get(Config.Excel.TEXT_OPENING_SKIP_CONFIRM);
        vm.RescueLabel = CommonText.Get(Config.Excel.TEXT_OPENING_RESCUE);
        vm.ClaimLabel = CommonText.Get(Config.Excel.TEXT_OPENING_CLAIM);
        vm.RetryLabel = CommonText.Get(Config.Excel.TEXT_OPENING_RETRY);
        vm.ErrorText = CommonText.Get(Config.Excel.TEXT_OPENING_VIDEO_ERROR);

        return true;
    }

    /// <summary>Sends the intent represented by the currently displayed panel.</summary>
    /// <param name="button">The authored source button.</param>
    /// <returns>Whether the UI sender accepted the intention.</returns>
    private bool OnContinue(Button button)
    {
        if (villageVisible)
            return Send<ReqHomeVillageAction>(button);

        return Send<ReqHomeOpeningContinue>(button);
    }

    /// <summary>Resolves the stage objective through the product language table.</summary>
    /// <param name="opening">The rescue state.</param>
    /// <param name="task">The selected task.</param>
    /// <returns>The localized current objective.</returns>
    private string GetObjective(HomeOpening opening, TaskEntity task)
    {
        if (opening.fetchingWater)
            return CommonText.Get(Config.Excel.TEXT_OPENING_WATER);

        if (opening.canOperate)
            return string.Format(CommonText.Get(opening.rescue.ObjectiveTextID), opening.rescue.WaterCost);

        if (opening.taskCompleted)
            return CommonText.Get(opening.rescue.AutoClaim != 0
                ? Config.Excel.TEXT_OPENING_AUTOMATIC_CLAIM : Config.Excel.TEXT_OPENING_CLAIM);

        return string.Format(CommonText.Get(Config.Excel.TEXT_OPENING_NEXT), task.configuration.ID,
            Config.Excel.propLanguageById(int.Parse(task.configuration.Description)));
    }

    /// <summary>Starts the authored clip and waits for actual decoder readiness.</summary>
    private void PlayMovie()
    {
        videoTexture = new RenderTexture((int)movie.clip.width, (int)movie.clip.height, 0);
        videoTexture.name = "OpeningVideo";
        videoTexture.Create();
        screen.texture = videoTexture;
        movie.targetTexture = videoTexture;
        movie.prepareCompleted += OnPrepared;
        movie.loopPointReached += OnMovieFinished;
        movie.errorReceived += OnMovieError;
        movie.Prepare();
    }

    /// <summary>Releases native video resources and all decoder callbacks.</summary>
    private void StopMovie()
    {
        movie.prepareCompleted -= OnPrepared;
        movie.loopPointReached -= OnMovieFinished;
        movie.errorReceived -= OnMovieError;
        movie.Stop();
        music.Stop();
        movie.targetTexture = null;
        screen.texture = null;
        if (videoTexture != null)
        {
            videoTexture.Release();
            Object.Destroy(videoTexture);
            videoTexture = null;
        }
    }

    /// <summary>Plays only after the video decoder is ready.</summary>
    /// <param name="player">The prepared player.</param>
    private void OnPrepared(VideoPlayer player)
    {
        player.Play();
        music.Play();
    }

    /// <summary>Completes the actual movie without completing any task.</summary>
    /// <param name="player">The finished player.</param>
    private void OnMovieFinished(VideoPlayer player)
    {
        vm.ConfirmVisible = false;
        XPluginUtility.PostEvent(new ReqHomeOpeningContinue());
    }

    /// <summary>Exposes decoder failure with retry and explicit skip controls.</summary>
    /// <param name="player">The failed player.</param>
    /// <param name="message">The native decoder error.</param>
    private void OnMovieError(VideoPlayer player, string message)
    {
        vm.ErrorVisible = true;
        Debug.LogWarning(message);
    }

    /// <summary>Requests skip confirmation while pausing playback.</summary>
    /// <param name="button">The clicked UI control.</param>
    /// <returns>Whether the intent was handled.</returns>
    private bool OnSkip(Button button)
    {
        movie.Pause();
        music.Pause();
        vm.ConfirmVisible = true;

        return true;
    }

    /// <summary>Confirms skipping only the movie.</summary>
    /// <param name="button">The clicked UI control.</param>
    /// <returns>Whether the intent was handled.</returns>
    private bool OnConfirm(Button button)
    {
        vm.ConfirmVisible = false;
        XPluginUtility.PostEvent(new ReqHomeOpeningContinue());

        return true;
    }

    /// <summary>Dismisses confirmation and resumes the movie.</summary>
    /// <param name="button">The clicked UI control.</param>
    /// <returns>Whether the intent was handled.</returns>
    private bool OnCancel(Button button)
    {
        vm.ConfirmVisible = false;
        movie.Play();
        music.UnPause();

        return true;
    }

    /// <summary>Retries the decoder without changing opening progress.</summary>
    /// <param name="button">The clicked UI control.</param>
    /// <returns>Whether the intent was handled.</returns>
    private bool OnRetry(Button button)
    {
        StopMovie();
        vm.ErrorVisible = false;
        PlayMovie();

        return true;
    }
}
