using Oathx.Core;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>Acknowledges an original conversation line.</summary>
public sealed class ReqRecruitContinue : UIRequestEvent { }

/// <summary>Closes the recruitment conversation.</summary>
public sealed class ReqRecruitClose : UIRequestEvent { }

/// <summary>Reports actual completion of the original recruitment video.</summary>
public sealed class ReqRecruitMovieFinished : UIRequestEvent { }

/// <summary>Projects dialogue and plays the recovered recruitment video.</summary>
[Config("Assets/Data/Widgets/Recruit/UIRecruit.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIRecruit : UIWidget<VMRecruit>
{
    /// <summary>Authored decoder with the original video clip.</summary>
    [SerializeField]
    private VideoPlayer movie;

    /// <summary>Authored movie surface.</summary>
    [SerializeField]
    private RawImage screen;

    /// <summary>Stops decoder callbacks before presentation is destroyed.</summary>
    public override void Deactive()
    {
        StopMovie();

        base.Deactive();
    }

    /// <summary>Releases owned video output when unbound.</summary>
    /// <param name="model">Released view model.</param>
    public override void Unref(VM model)
    {
        StopMovie();

        base.Unref(model);
    }

    /// <summary>Displays a configured original conversation row.</summary>
    /// <param name="row">Current original line.</param>
    public void Invalidate(Config.RecruitDialogue row)
    {
        vm.DialogueVisible = true;
        vm.MovieVisible = false;
        vm.RetryVisible = false;
        vm.Speaker = CommonText.Get(row.SpeakerTextID);
        vm.Dialogue = CommonText.Get(row.ContentTextID);
        vm.Portrait = row.PortraitPath;
    }

    /// <summary>Displays a rejected whistle operation without completing anything.</summary>
    public void ShowFailure()
    {
        vm.Dialogue = CommonText.Get(Config.Excel.TEXT_RECRUIT_MISSING);
    }

    /// <summary>Plays or retries the paid recruitment movie.</summary>
    public void PlayMovie()
    {
        StopMovie();
        vm.DialogueVisible = false;
        vm.MovieVisible = true;
        vm.RetryVisible = false;
        movie.loopPointReached += OnMovieFinished;
        movie.errorReceived += OnMovieError;
        movie.prepareCompleted += OnPrepared;
        movie.Prepare();
    }

    /// <summary>Installs explicit UI intentions.</summary>
    /// <returns>Whether bindings succeeded.</returns>
    protected override bool Bind()
    {
        vm.ContinueClick = Send<ReqRecruitContinue>;
        vm.CloseClick = Send<ReqRecruitClose>;
        vm.RetryClick = OnRetry;

        return true;
    }

    /// <summary>Reports actual playback completion.</summary>
    /// <param name="player">Decoder that reached its final frame.</param>
    private void OnMovieFinished(VideoPlayer player)
    {
        XPluginUtility.PostEvent(new ReqRecruitMovieFinished());
    }

    /// <summary>Starts playback only after the original clip is decoded.</summary>
    /// <param name="player">Prepared decoder.</param>
    private void OnPrepared(VideoPlayer player)
    {
        screen.texture = player.texture;
        player.Play();
    }

    /// <summary>Retries only playback of the already paid movie.</summary>
    /// <param name="button">Actual retry control.</param>
    /// <returns>Whether the explicit click was accepted.</returns>
    private bool OnRetry(Button button)
    {
        PlayMovie();

        return true;
    }

    /// <summary>Allows explicit retry while preserving the paid operation.</summary>
    /// <param name="player">Failed decoder.</param>
    /// <param name="error">Decoder error.</param>
    private void OnMovieError(VideoPlayer player, string error)
    {
        Debug.LogError(error);
        vm.RetryVisible = true;
    }

    /// <summary>Releases decoder callbacks and its owned texture.</summary>
    private void StopMovie()
    {
        movie.loopPointReached -= OnMovieFinished;
        movie.errorReceived -= OnMovieError;
        movie.prepareCompleted -= OnPrepared;
        movie.Stop();
        screen.texture = null;
    }
}
