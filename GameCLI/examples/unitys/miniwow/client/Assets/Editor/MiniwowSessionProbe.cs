using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Exercises the local entry and master session contract through the Unity bridge.</summary>
public static class MiniwowSessionProbe
{
    /// <summary>Login instance that must remain visible until Home is ready.</summary>
    private static int transitionLogin;

    /// <summary>Number of observed editor frames during the handoff.</summary>
    private static int transitionFrames;

    /// <summary>Whether the observed handoff exposed a missing entry page.</summary>
    private static bool transitionGap;

    /// <summary>Whether Home completed the handoff and Login was removed.</summary>
    private static bool transitionComplete;

    /// <summary>Begins frame-by-frame observation of the existing Login instance.</summary>
    public static void BeginHomeTransitionCheck()
    {
        UISystem.GetSingleton().TryGetWidget<UILogin>(out var login);
        transitionLogin = login.GetInstanceID();
        transitionFrames = 0;
        transitionGap = false;
        transitionComplete = false;
        UnityEditor.EditorApplication.update += OnObserveHomeTransition;
    }

    /// <summary>Checks that Login stayed visible until the actual Home page and scene became ready.</summary>
    /// <returns>The observed handoff evidence.</returns>
    public static string VerifyHomeTransition()
    {
        UnityEditor.EditorApplication.update -= OnObserveHomeTransition;
        if (transitionGap || !transitionComplete || transitionFrames == 0)
            throw new InvalidOperationException("Login was not retained until Home readiness.");

        return JsonConvert.SerializeObject(new {
            frames = transitionFrames,
            loginInstance = transitionLogin,
            gap = transitionGap,
            complete = transitionComplete
        });
    }

    /// <summary>Exercises public lifecycle events and returns the resident coordinator identity.</summary>
    /// <param name="id">The configured business system identifier.</param>
    /// <param name="activate">Whether to activate or unload the system.</param>
    /// <returns>The coordinator identity before and after dispatch.</returns>
    public static string VerifySystemEvent(int id, bool activate)
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        var master = plugin.GetMaster();
        if (activate)
            plugin.DispatchEvent(new SystemActive { id = id });
        else
            plugin.DispatchEvent(new SystemDetive { id = id });

        if (!ReferenceEquals(master, plugin.GetMaster()) || !master.IsActive())
            throw new InvalidOperationException("A business lifecycle event replaced or stopped Master.");

        return master.GetHashCode().ToString();
    }

    /// <summary>Checks configuration completeness and source selection without saving test changes.</summary>
    /// <returns>The outcomes of malformed resource and missing-directory checks.</returns>
    public static string VerifyConfigurationSource()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        var configure = XApp.GetSingleton().GetAppConfigure();
        string originalPath = configure.cfgBundle;
        var originalSource = configure.bundleSource;
        var reader = typeof(GamePlugin).GetMethod("ReadConfigFiles",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var assembly = typeof(GamePlugin).Assembly;
        var assets = UnityEditor.AssetDatabase.FindAssets("t:TextAsset", new[] { originalPath })
            .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => assembly.GetType("Config." + asset.name) != null).ToArray();
        bool emptyRejected = false;
        bool missingRejected = false;
        bool duplicateRejected = false;
        bool missingDirectoryBlocked = false;
        try
        {
            plugin.Shutdown();
            emptyRejected = !(bool)reader.Invoke(plugin, new object[] { assembly, Array.Empty<TextAsset>() });
            missingRejected = !(bool)reader.Invoke(plugin, new object[] { assembly, assets.Skip(1).ToArray() });
            duplicateRejected = !(bool)reader.Invoke(plugin,
                new object[] { assembly, assets.Concat(assets.Take(1)).ToArray() });
            configure.bundleSource = AppBundleSource.Local;
            configure.cfgBundle = originalPath + "/MissingConfigurationProbe";
            if (!plugin.Install())
                throw new InvalidOperationException("Could not install plugin for missing-directory check.");

            plugin.Startup();
            missingDirectoryBlocked = plugin.GetMaster() == null && MiniwowTownProbe.GetRuntime() == null;
        }
        finally
        {
            plugin.Shutdown();
            configure.cfgBundle = originalPath;
            configure.bundleSource = originalSource;
            if (!plugin.Install() || !plugin.Startup())
                throw new InvalidOperationException("Could not restore configured startup after probe.");
        }

        if (!emptyRejected || !missingRejected || !duplicateRejected || !missingDirectoryBlocked)
            throw new InvalidOperationException("Configuration source contract failed.");

        return JsonConvert.SerializeObject(new {
            emptyRejected, missingRejected, duplicateRejected, missingDirectoryBlocked, restoredPath = originalPath
        });
    }

    /// <summary>Persists the local configuration directory through Unity asset APIs.</summary>
    /// <returns>The configured directory.</returns>
    public static string ConfigureSource()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Configuration authoring requires edit mode.");

        var configure = UnityEditor.AssetDatabase.LoadAssetAtPath<XAppConfigure>(
            "Assets/Resources/XAppConfigure.asset");
        configure.cfgBundle = "Assets/Data/Jsons";
        UnityEditor.EditorUtility.SetDirty(configure);
        UnityEditor.AssetDatabase.SaveAssets();

        return configure.cfgBundle;
    }

    /// <summary>Checks plugin teardown before accepting a fresh asynchronous startup.</summary>
    /// <returns>Whether infrastructure installation and startup were accepted.</returns>
    public static bool RestartPlugin()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        plugin.Shutdown();
        if (plugin.GetMaster() != null || MiniwowTownProbe.GetRuntime() != null || XEntityManager.GetSingleton().TryGetEntity(nameof(MasterEntity), out _) || XEntityManager.GetSingleton().TryGetEntity(nameof(HomeEntity), out _))
            throw new InvalidOperationException("Plugin shutdown retained modules or entities.");

        return plugin.Install() && plugin.Startup();
    }

    /// <summary>Inspects actual entry text components and source-font character support.</summary>
    /// <returns>Text and font diagnostics without changing assets.</returns>
    public static string InspectFonts()
    {
        UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget);

        return JsonConvert.SerializeObject(widget.GetComponentsInChildren<Graphic>(true).Select(graphic => new {
            type = graphic.GetType().Name,
            name = graphic.name,
            font = graphic is Text text && text.font ? text.font.name : null,
            content = graphic is Text label ? label.text : null,
            hasStart = graphic is Text value && value.font && value.font.HasCharacter('\u5f00')
        }));
    }

    /// <summary>Cancels an entering generation before its deferred module load can commit.</summary>
    /// <param name="restart">Whether to stop and reactivate Master instead of returning.</param>
    /// <returns>Whether the entering generation was explicitly canceled.</returns>
    public static bool CancelEntering(bool restart)
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        plugin.DispatchEvent(new LoginSucceed());
        if (restart)
            StopMaster(true);
        else
            plugin.DispatchEvent(new ReturnToLogin());

        return true;
    }

    /// <summary>Reports actual module, page and scene ownership.</summary>
    /// <returns>A serialized session observation.</returns>
    public static string Snapshot()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        var master = plugin.GetMaster();
        var login = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        bool loginOpen = UISystem.GetSingleton().TryGetWidget<UILogin>(out var entry);
        bool homeOpen = UISystem.GetSingleton().TryGetWidget<UIHome>(out _);
        var town = MiniwowTownProbe.GetRuntime();

        return JsonConvert.SerializeObject(new {
            phase = !master.IsActive() ? "Stopped" : loginOpen ? "Login" : homeOpen ? "Town" : "Waiting",
            loginPhase = login ? login.phase.ToString() : "Missing",
            loginOpen,
            loginInstances = UnityEngine.Object.FindObjectsOfType<UILogin>().Length,
            loginInstance = entry ? entry.GetInstanceID() : 0,
            homeOpen,
            townReady = town != null && town.IsReady(),
            input = UnityEngine.Object.FindObjectOfType<HomeScene>()?.AcceptsInput() ?? false,
            launchVisible = UnityEngine.Object.FindObjectsOfType<XLauncher>().Any(),
            startVisible = entry && entry.vm && entry.vm.EnterVisible,
            failureVisible = entry && entry.vm && entry.vm.FailedVisible
        });
    }

    /// <summary>Clicks the actual start button and checks duplicate request rejection.</summary>
    /// <returns>The resulting state before asynchronous completion.</returns>
    public static string ClickStart()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            throw new InvalidOperationException("Login must be open.");

        Button button = widget.GetComponentsInChildren<Button>().Single(item => item.name == "EnterClick");
        RectTransform rect = button.GetComponent<RectTransform>();
        var pointer = new PointerEventData(EventSystem.current) {
            position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
            button = PointerEventData.InputButton.Left
        };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
            throw new InvalidOperationException("Start is not the first clickable raycast target.");

        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        if (XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<LoginSystem>().EnterGame())
            throw new InvalidOperationException("Repeated entry was accepted.");

        return Snapshot();
    }

    /// <summary>Requests the same return contract used by future product exit intents.</summary>
    /// <returns>Whether the coordinator accepted teardown.</returns>
    public static bool Return()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        plugin.DispatchEvent(new ReturnToLogin());

        return true;
    }

    /// <summary>Injects a reversible invalid configuration only into the in-memory test fixture.</summary>
    /// <param name="invalid">Whether to invalidate the configured player speed.</param>
    public static void SetInvalidConfiguration(bool invalid)
    {
        var config = Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER);
        var source = JsonConvert.DeserializeObject<Config.ActorConfig>(System.IO.File.ReadAllText("Assets/Data/Jsons/Excels/ActorConfig.bytes"));
        typeof(Config.Actor).GetProperty(nameof(Config.Actor.MoveSpeed)).SetValue(config, invalid ? 0f : source.data[Config.Excel.ACTOR_TOWN_PLAYER].MoveSpeed);
    }

    /// <summary>Checks that duplicate activation cannot reset a live session.</summary>
    /// <returns>Whether phase and entry state remained unchanged.</returns>
    public static bool VerifyDuplicateActivation()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        string before = Snapshot();
        plugin.GetMaster().Active();
        if (UISystem.GetSingleton().TryGetWidget<UILogin>(out _))
            plugin.Query<LoginSystem>().Active();
        if (before != Snapshot())
            throw new InvalidOperationException("Duplicate activation reset the session.");

        return true;
    }

    /// <summary>Stops and optionally reactivates Master to exercise owned-module cleanup.</summary>
    /// <param name="restart">Whether to reactivate the coordinator immediately.</param>
    public static void StopMaster(bool restart)
    {
        var master = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetMaster();
        master.Detive();
        if (restart)
        {
            master.Active();

            var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
            plugin.DispatchEvent(new ReturnToLogin());
        }
    }
    /// <summary>Renders the live scene and active canvases when desktop capture is stale.</summary>
    /// <returns>The native render output path; this is not a desktop screenshot.</returns>
    public static string RenderPreview()
    {
        Camera sceneCamera = UnityEngine.Object.FindObjectsOfType<Camera>().First();
        Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>().Where(item => item.isRootCanvas).ToArray();
        var modes = canvases.Select(item => item.renderMode).ToArray();
        var cameras = canvases.Select(item => item.worldCamera).ToArray();
        var planes = canvases.Select(item => item.planeDistance).ToArray();
        var target = new RenderTexture(Screen.width, Screen.height, 24);
        var uiObject = new GameObject("LoginPreviewCamera");
        Camera uiCamera = uiObject.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = -1;
        uiCamera.targetTexture = target;
        RenderTexture previous = RenderTexture.active;
        RenderTexture priorTarget = sceneCamera.targetTexture;
        Texture2D image = null;
        try
        {
            sceneCamera.targetTexture = target;
            sceneCamera.Render();
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.planeDistance = 1f;
            }

            Canvas.ForceUpdateCanvases();
            uiCamera.Render();
            RenderTexture.active = target;
            image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
            image.Apply();
            System.IO.Directory.CreateDirectory("Library/Login");
            System.IO.File.WriteAllBytes("Library/Login/native-preview.png", image.EncodeToPNG());
        }
        finally
        {
            for (int index = 0; index < canvases.Length; index++)
            {
                canvases[index].renderMode = modes[index];
                canvases[index].worldCamera = cameras[index];
                canvases[index].planeDistance = planes[index];
            }

            sceneCamera.targetTexture = priorTarget;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(uiObject);
            UnityEngine.Object.DestroyImmediate(target);
            if (image)
                UnityEngine.Object.DestroyImmediate(image);

            Canvas.ForceUpdateCanvases();
        }

        return "Library/Login/native-preview.png";
    }

    /// <summary>Samples real runtime and widget ownership throughout the login handoff.</summary>
    private static void OnObserveHomeTransition()
    {
        transitionFrames++;
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        var home = plugin.QuerySystem<HomeSystem>();
        bool homeReady = home != null && home.IsReady();
        bool loginOpen = UISystem.GetSingleton().TryGetWidget<UILogin>(out var login);
        if (!homeReady && (!loginOpen || login.GetInstanceID() != transitionLogin))
            transitionGap = true;

        if (homeReady && !loginOpen)
        {
            transitionComplete = true;
            UnityEditor.EditorApplication.update -= OnObserveHomeTransition;
        }
    }

}
