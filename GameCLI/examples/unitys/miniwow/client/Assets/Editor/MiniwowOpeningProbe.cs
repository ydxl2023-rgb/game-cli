using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>Exercises real opening controls and movement without fabricating task progress.</summary>
public static class MiniwowOpeningProbe
{
    /// <summary>Reports objects that must disappear when Home is unloaded.</summary>
    /// <returns>Owned presentation counts after returning to login.</returns>
    public static string CleanupSnapshot()
    {
        return JsonConvert.SerializeObject(new {
            movies = Object.FindObjectsOfType<VideoPlayer>().Length,
            overlays = Object.FindObjectsOfType<UIHomeOpening>().Length,
            scenes = Object.FindObjectsOfType<HomeScene>().Length,
            runtimes = Object.FindObjectsOfType<HomeRuntime>().Length,
            textures = Resources.FindObjectsOfTypeAll<RenderTexture>().Count(item => item.name == "OpeningVideo")
        });
    }

    /// <summary>Returns current entity, scene, video and task evidence.</summary>
    /// <returns>A detached runtime snapshot.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var player = home.FindActor(4);
        var movie = Object.FindObjectOfType<VideoPlayer>();
        var nodes = Object.FindObjectOfType<HomeScene>().gameObject.scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var merchant = nodes.Single(item => item.name == "NPC_auction_2");
        var guide = nodes.Single(item => item.name == "GuideObjs");
        var animation = merchant.GetComponent<Spine.Unity.SkeletonAnimation>();
        var music = movie ? movie.GetComponent<AudioSource>() : null;

        return JsonConvert.SerializeObject(new {
            phase = home.opening?.phase.ToString(),
            water = home.opening?.water,
            task = tasks.GetTrackedTask()?.configuration.ID,
            first = tasks.GetTask(60001).status.ToString(),
            second = tasks.GetTask(60002).status.ToString(),
            third = tasks.GetTask(60003).status.ToString(),
            hospitalCleared = nodes.Single(item => item.name == "priest_initial_3").gameObject.activeInHierarchy,
            hospitalInitial = nodes.Single(item => item.name == "priest_initial").gameObject.activeInHierarchy,
            clearingSmoke = nodes.Single(item => item.name == "ClearingSmoke").GetComponent<ParticleSystem>().isPlaying,
            countdown = Object.FindObjectOfType<UIHomeOpening>().GetComponentsInChildren<Text>(true)
                .Single(item => item.name == "WaterCount").text,
            hotelDamaged = nodes.Single(item => item.name == "Hotel_initial_1").gameObject.activeInHierarchy,
            hotelRescued = nodes.Single(item => item.name == "Hotel_initial").gameObject.activeInHierarchy,
            hotelOwnerVisible = nodes.Single(item => item.name == "NPC_hotel").gameObject.activeInHierarchy,
            hospitalDamaged = nodes.Single(item => item.name == "priest_initial_1").gameObject.activeInHierarchy,
            hospitalRescued = nodes.Single(item => item.name == "priest_initial_2").gameObject.activeInHierarchy,
            damagedModel = nodes.Single(item => item.name == "auction_initial_1").gameObject.activeInHierarchy,
            restoredModel = nodes.Single(item => item.name == "auction_initial").gameObject.activeInHierarchy,
            merchantVisible = merchant.gameObject.activeInHierarchy,
            merchantEntity = home.FindActor(5) != null,
            merchantAnimation = animation.AnimationState.GetCurrent(0)?.Animation.Name,
            wellAvailable = guide.Find("Money1").gameObject.activeSelf,
            wellCollected = guide.Find("Money5").gameObject.activeSelf,
            position = player == null ? null : new[] { player.position.x, player.position.y, player.position.z },
            videoTime = movie ? movie.time : 0,
            playing = movie && movie.isPlaying,
            prepared = movie && movie.isPrepared,
            videoLength = movie && movie.clip ? movie.clip.length : 0,
            audioTracks = movie && movie.clip ? movie.clip.audioTrackCount : 0,
            musicPlaying = movie && movie.GetComponent<AudioSource>().isPlaying,
            musicClip = music && music.clip ? music.clip.name : null,
            fire = Object.FindObjectOfType<HomeOpeningScene>().GetComponentsInChildren<ParticleSystem>().Length,
            buttons = Object.FindObjectOfType<UIHomeOpening>().GetComponentsInChildren<Button>()
                .Select(item => item.name)

        });
    }

    /// <summary>Clicks the actual authored opening button.</summary>
    /// <param name="name">The button node name.</param>
    public static void Click(string name)
    {
        var widget = Object.FindObjectOfType<UIHomeOpening>();
        var button = widget.GetComponentsInChildren<Button>().Single(item => item.name == name);
        button.onClick.Invoke();
    }

    /// <summary>Moves through the real input adapter until entering the original well volume.</summary>
    public static void WalkToWater()
    {
        XCoroutine.Run(Walk());
    }

    /// <summary>Clears only this feature's local test progress for a fresh playthrough.</summary>
    public static void ResetLocalSave()
    {
        PlayerPrefs.DeleteKey(nameof(HomeEntity) + ".opening");
        PlayerPrefs.DeleteKey(nameof(TaskCollectionEntity));
        PlayerPrefs.DeleteKey(StorageKey.For<HomeSystem, HomeEntity>(entity => entity.opening));
        PlayerPrefs.DeleteKey(StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId));
        PlayerPrefs.Save();
    }

    /// <summary>Captures the actual rendered resolution rather than the editor window crop.</summary>
    public static void Capture()
    {
        ScreenCapture.CaptureScreenshot("Library/OpeningQA/full-resolution.png");
    }

    /// <summary>Feeds analog movement rather than modifying actor positions.</summary>
    /// <returns>The movement verification coroutine.</returns>
    private static IEnumerator Walk()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var runtime = RuntimeManager.GetSingleton().QueryRuntime<HomeRuntime>();
        var config = home.opening.configuration;
        var target = new Vector3(config.WaterX, config.WaterY, config.WaterZ);
        float elapsed = 0f;
        while (home.opening.phase == HomeOpeningPhase.FetchWater && elapsed < 10f)
        {
            var actor = home.FindActor(4);
            Vector3 direction = target - actor.position;
            direction.y = 0f;
            runtime.SetPlayerInput(direction.normalized);
            elapsed += Time.deltaTime;

            yield return null;
        }

        runtime.SetPlayerInput(Vector3.zero);
    }
}
