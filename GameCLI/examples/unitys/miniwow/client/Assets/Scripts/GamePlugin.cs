using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Oathx.Bundle.Assets;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Installs product infrastructure and manages configured module lifecycles.</summary>
public sealed class GamePlugin : XPlugin
{
    /// <summary>Invalidates configuration callbacks after shutdown or another startup.</summary>
    private int startupGeneration;

    /// <summary>Whether the current startup owns runtime resources.</summary>
    private bool running;

    /// <summary>Installs the UI infrastructure used by this offline product.</summary>
    /// <returns>Whether installation succeeded.</returns>
    public override bool Install()
    {
        InstallUISystem();

        return true;
    }

    /// <summary>Requests asynchronous configuration loading before entity and system installation.</summary>
    /// <returns>Whether startup was accepted, not whether asynchronous readiness completed.</returns>
    public override bool Startup()
    {
        if (running)
            return true;

        running = true;
        startupGeneration++;
        XApp.GetSingleton().ShowLaunchBackground();
        bool accepted = TryLoadConfigFilesAsync(succeeded => {
            if (!succeeded)
            {
                running = false;
                Debug.LogError("MINIWOW_CONFIG_LOAD_FAILED");

                return;
            }

            InstallEntities();
            InstallSystems();
        });
        if (!accepted)
            running = false;

        return accepted;
    }

    /// <summary>Updates installed modules and their logical entities.</summary>
    /// <param name="deltaTime">Elapsed frame time.</param>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (!running)
            return;

        XEntityManager.GetSingleton().Update(deltaTime);
    }

    /// <summary>Stops business modules before releasing entities and UI services.</summary>
    public override void Shutdown()
    {
        running = false;
        startupGeneration++;

        UninstallSystems();
        RuntimeManager.GetSingleton().Shutdown();

        UninstallEntities();
        UninstallUISystem();
    }

    /// <summary>Releases installation-only registrations; this product has no SDK or RPC.</summary>
    public override void Uninstall()
    {
    }

    /// <summary>Installs a configured module and rolls back unsuccessful activation.</summary>
    /// <param name="type">The configured runtime module type.</param>
    /// <returns>The installed module.</returns>
    public override XEventModule Load(System.Type type)
    {
        if (!TryGetFunctionSystem(type, out var configuration))
            throw new System.InvalidOperationException($"No valid system configuration for {type.FullName}.");

        if (removes.Contains(type.Name))
            throw new System.InvalidOperationException($"System {type.FullName} is pending removal.");

        bool existing = modules.TryGetValue(type.Name, out var module);
        if (existing && module.GetType() != type)
            throw new System.InvalidOperationException(
                $"System name {type.Name} is already registered to another type.");

        try
        {
            if (!existing)
            {
                module = (XEventModule)System.Activator.CreateInstance(type);
                module.SetDispatcher(this);
                if (!module.Install())
                    throw new System.InvalidOperationException($"System {type.FullName} refused installation.");

                modules.Add(type.Name, module);
            }

            if (configuration.Active != 0 && !module.Active())
                throw new System.InvalidOperationException($"System {type.FullName} refused activation.");

            return module;
        }
        catch
        {
            if (existing)
                Unload(type);
            else if (module != null)
            {
                ReleaseSystem(module);
                modules.Remove(type.Name);
            }

            throw;
        }
    }

    /// <summary>Uses the same lifecycle policy as the runtime-type overload.</summary>
    /// <typeparam name="T">The configured module type.</typeparam>
    /// <returns>The installed module.</returns>
    public override T Load<T>()
    {
        return (T)Load(typeof(T));
    }

    /// <summary>Stops and uninstalls a module once before deferred removal.</summary>
    /// <param name="type">The module type to release.</param>
    public override void Unload(System.Type type)
    {
        if (removes.Contains(type.Name) || !modules.TryGetValue(type.Name, out var module))
            return;

        ReleaseSystem(module);
        Remove(type.Name);
    }

    /// <summary>Uses the same teardown policy as the runtime-type overload.</summary>
    /// <typeparam name="T">The module type to release.</typeparam>
    public override void Unload<T>()
    {
        Unload(typeof(T));
    }

    /// <summary>Uninstalls every registered module and clears pending removals.</summary>
    public override void UnloadAll()
    {
        foreach (var module in modules.Values.ToArray())
            Unload(module.GetType());

        modules.Clear();
        removes.Clear();
    }

    /// <summary>Returns the installed coordinator for editor diagnostics.</summary>
    /// <returns>The registered coordinator, or null.</returns>
    public MasterSystem GetMaster()
    {
        return QuerySystem<MasterSystem>();
    }

    /// <summary>Queries the existing registry without loading another module.</summary>
    /// <typeparam name="T">The module type.</typeparam>
    /// <returns>The registered module, or null.</returns>
    public T QuerySystem<T>() where T : RuntimeSystem
    {
        return modules.TryGetValue(typeof(T).Name, out var module) ? module as T : null;
    }

    /// <summary>Attempts both teardown stages even when a module reports an exception.</summary>
    /// <param name="module">The installed or partially installed module.</param>
    private void ReleaseSystem(XEventModule module)
    {
        try
        {
            if (module.IsActive())
                module.Detive();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }

        try
        {
            module.Uninstall();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    /// <summary>Configures the product's independent UI adapters.</summary>
    private void InstallUISystem()
    {
        MetadataSyncFactory.GetSingleton().Register(nameof(SyncAnchoredPosition), new SyncAnchoredPosition());

        var configure = new ResourceConfigure(new GameUISender()) {
            loader = new GameUILoader(),
            cacher = new UIDefaultCacher()
        };

        UISystem.GetSingleton().Startup(configure);
    }

    /// <summary>Releases the UI services after business modules stop.</summary>
    private void UninstallUISystem()
    {
        UISystem.GetSingleton().Shutdown();
    }

    /// <summary>Installs entities explicitly marked for startup.</summary>
    private void InstallEntities()
    {
        var types = new List<System.Type>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsAbstract || !typeof(XEntity).IsAssignableFrom(type))
                continue;
            if (type.GetCustomAttributes(typeof(Initialize), true)
                .Cast<Initialize>()
                .Any(attribute => attribute.InstallType == InstallType.Startup))
                types.Add(type);
        }

        XEntityManager.GetSingleton().Startup(types);
    }

    /// <summary>Destroys runtime entities only when the entire plugin shuts down.</summary>
    private void UninstallEntities()
    {
        XEntityManager.GetSingleton().Shutdown();
    }

    /// <summary>Installs startup modules selected by the product configuration.</summary>
    private void InstallSystems()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        foreach (var row in Config.Excel.functionSystemConfig.Values)
        {
            if (row.InstallOnStartup != 0)
            {
                var type = assembly.GetType(row.ClassType);
                if (type != null)
                    Load(type);
            }
        }
    }

    /// <summary>Stops every module owned by this plugin.</summary>
    private void UninstallSystems()
    {
        UnloadAll();
    }

    /// <summary>Finds a configured runtime type without throwing for an ordinary missing entry.</summary>
    /// <param name="type">The requested type.</param>
    /// <param name="configuration">The matching configuration entry.</param>
    /// <returns>Whether a matching concrete runtime module is configured.</returns>
    private bool TryGetFunctionSystem(System.Type type, out Config.FunctionSystem configuration)
    {
        configuration = null;
        if (type.IsAbstract || !typeof(RuntimeSystem).IsAssignableFrom(type))
            return false;

        configuration = Config.Excel.functionSystemConfig.Values.FirstOrDefault(row => row.ClassType == type.FullName);

        return configuration != null;
    }

    /// <summary>Loads the configured directory or bundle using the reference project's resource entry.</summary>
    /// <param name="complete">Receives the current startup result.</param>
    /// <returns>Whether a supported configuration load was accepted.</returns>
    private bool TryLoadConfigFilesAsync(System.Action<bool> complete)
    {
        var configure = XApp.GetSingleton().GetAppConfigure();
        if (!configure || string.IsNullOrWhiteSpace(configure.cfgBundle))
        {
            Debug.LogError("MINIWOW_CONFIG_SOURCE_MISSING");

            return false;
        }

        int generation = startupGeneration;
        Assembly assembly = Assembly.GetExecutingAssembly();
        if (configure.bundleSource == AppBundleSource.Local)
        {
#if UNITY_EDITOR
            LoadConfigDirectory<TextAsset>(configure.cfgBundle, assets => {
                if (running && generation == startupGeneration)
                    complete(ReadConfigFiles(assembly, assets));
            });
#else
            Debug.LogError("MINIWOW_LOCAL_CONFIG_REQUIRES_EDITOR");

            return false;
#endif
        }
        else
        {
            XCoroutine.Run(LoadConfigBundleAsync(configure.cfgBundle, assembly, generation, complete));
        }

        return true;
    }

    /// <summary>Waits for the bundle operation even when the resource loader cannot invoke its callback.</summary>
    /// <param name="path">Bundle name from the application configuration.</param>
    /// <param name="assembly">Assembly containing generated configuration types.</param>
    /// <param name="generation">The owning startup generation.</param>
    /// <param name="complete">Receives the current startup result.</param>
    /// <returns>The bundle loading operation.</returns>
    private IEnumerator LoadConfigBundleAsync(
        string path, Assembly assembly, int generation, System.Action<bool> complete)
    {
        Object[] assets = null;
        yield return XRes.LoadBundleAsync(path, bundle => {
            if (running && generation == startupGeneration && bundle)
                assets = bundle.LoadAllAssets();
        });
        if (!running || generation != startupGeneration)
            yield break;

        complete(ReadConfigFiles(assembly, assets));
    }

    /// <summary>Maps asset names to generated types and writes their data into the Excel fields.</summary>
    /// <param name="assembly">Assembly containing generated configuration types.</param>
    /// <param name="assets">Configuration resources from the selected directory or bundle.</param>
    /// <returns>Whether every generated Excel field was loaded successfully.</returns>
    private bool ReadConfigFiles(Assembly assembly, Object[] assets)
    {
        if (assets == null || assets.Length == 0 || !TryGetExcelFields(out var fields))
            return false;

        var loaded = new HashSet<string>();
        try
        {
            foreach (var resource in assets)
            {
                if (!(resource is TextAsset asset))
                    continue;

                var type = assembly.GetType(GetConfigTypeName(asset.name));
                if (type == null || !typeof(Config.IConfigData).IsAssignableFrom(type))
                    continue;

                var text = System.Text.Encoding.UTF8.GetString(asset.bytes);
                var configData = JsonConvert.DeserializeObject(text, type) as Config.IConfigData;
                if (configData == null || string.IsNullOrEmpty(configData.GetName()))
                    return false;
                if (!fields.TryGetValue(configData.GetName(), out var field))
                    return false;

                var data = configData.GetData();
                if (!field.FieldType.IsInstanceOfType(data) || !loaded.Add(field.Name))
                    return false;

                field.SetValue(null, data);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);

            return false;
        }

        return loaded.Count == fields.Count;
    }

    /// <summary>Collects the public static readonly fields generated by the Excel exporter.</summary>
    /// <param name="fields">Fields indexed by their generated names.</param>
    /// <returns>Whether the assembly contains configuration targets.</returns>
    private bool TryGetExcelFields(out Dictionary<string, FieldInfo> fields)
    {
        fields = new Dictionary<string, FieldInfo>();
        foreach (var field in typeof(Config.Excel).GetFields())
        {
            if (field.IsPublic && field.IsStatic && field.IsInitOnly)
                fields.Add(field.Name, field);
        }

        return fields.Count > 0;
    }

    /// <summary>Resolves the generated configuration namespace from a resource name.</summary>
    /// <param name="name">Text asset name without its extension.</param>
    /// <returns>The generated configuration type name.</returns>
    private string GetConfigTypeName(string name)
    {
        return $"Config.{name}";
    }

#if UNITY_EDITOR
    /// <summary>Reads local configuration resources through the Unity asset database.</summary>
    /// <typeparam name="T">The configuration resource type.</typeparam>
    /// <param name="path">Directory from the application configuration.</param>
    /// <param name="complete">Receives resources or an empty array for a missing directory.</param>
    private void LoadConfigDirectory<T>(string path, System.Action<T[]> complete) where T : Object
    {
        var assets = new List<T>();
        if (Directory.Exists(path))
        {
            var files = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });
            foreach (var file in files)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(file);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset)
                    assets.Add(asset);
            }
        }

        complete(assets.ToArray());
    }
#endif


}

/// <summary>Loads future widgets through the existing framework resource layer.</summary>
public sealed class GameUILoader : UILoader
{
    /// <summary>Loads a resource by its compile-time type.</summary>
    /// <param name="path">Resource path.</param>
    /// <param name="done">Completion callback.</param>
    /// <returns>The loading coroutine.</returns>
    public Coroutine LoadAsync<T>(string path, System.Action<Object> done) where T : Object
    {
        return Oathx.Bundle.Assets.XRes.LoadAsync<T>(path, done);
    }

    /// <summary>Loads a resource by its runtime type.</summary>
    /// <param name="path">Resource path.</param>
    /// <param name="type">Resource type.</param>
    /// <param name="done">Completion callback.</param>
    /// <returns>The loading coroutine.</returns>
    public Coroutine LoadAsync(string path, System.Type type, System.Action<Object> done)
    {
        return Oathx.Bundle.Assets.XRes.LoadAsync(path, type, done);
    }
}

/// <summary>Adapts widget intentions to synchronous plugin events or the application event queue.</summary>
public sealed class GameUISender : UISender
{
    /// <summary>Immediately dispatches application events through the plugin utility.</summary>
    /// <param name="request">The widget intention.</param>
    /// <returns>Whether the request implements the application event contract.</returns>
    public bool Send(IUIRequest request)
    {
        return request is IEvent message && XPluginUtility.FireEvent(message);
    }

    /// <summary>Queues valid application events through the existing event loop.</summary>
    /// <param name="request">The widget intention.</param>
    /// <returns>Whether the application accepted the event.</returns>
    public bool Post(IUIRequest request)
    {
        return request is IEvent message && XPluginUtility.PostEvent(message);
    }
}
