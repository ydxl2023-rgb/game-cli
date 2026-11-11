using Oathx.Bundle.Assets;
using Oathx.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Oathx.Entity;
using Oathx.Rpc;
using Config;
using Newtonsoft.Json;
using System.Reflection;
using UnityEditor;
using Oathx.Core.Sdk;
using Oathx.Core.Actitvity;
using Oathx.Widget;

/// <summary>
/// UI resource loader
/// </summary>
public class UIResourceLoader : UILoader
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="done"></param>
    /// <returns></returns>
    public Coroutine LoadAsync<T>(string path, System.Action<Object> done) where T : Object
    {
        return XRes.LoadAsync<T>(path, done);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <param name="done"></param>
    /// <returns></returns>
    public Coroutine LoadAsync(string path, System.Type type, System.Action<Object> done)
    {
        return XRes.LoadAsync(path, type, done);
    }
}

/// <summary>
/// Game Plugin Main Entry
/// </summary>
public class GamePlugin : XPlugin
{
    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns></returns>
    public override bool Install()
    {
        InstallSdkAPI();
        InstallNetRpc();
        InstallUISystem();

        return true;
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override void Uninstall()
    {
        UninstallNetRpc();
        UninstallSdkAPI();
    }

    /// <summary>
    /// Startup the plugin
    /// </summary>
    /// <returns></returns>
    public override bool Startup()
    {
        TryLoadAsyncConfiyFiles(() => {
            InstallEntitys();
            InstallSystems();
        });

        return true;
    }

    /// <summary>
    /// Install game systems
    /// </summary>
    private void InstallSystems()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        foreach (var it in Excel.functionSystemConfig)
        {
            if (it.Value.InstallOnStartup != 0)
            {
                var type = assembly.GetType(it.Value.ClassType);
                if (type != null)
                    Load(type);
            }
        }
    }

    /// <summary>
    /// Uninstall game systems
    /// </summary>
    private void UninstallSystems()
    {
        UnloadAll();
    }

    /// <summary>
    /// Update the plugin
    /// </summary>
    /// <param name="deltaTime"></param>
    public override void Update(float deltaTime)
    {
        // update netrpc
        XNetRpc.Update(deltaTime);

        // update entity dispatcher, fire all entity in queue, notify all listener        
        XEntityManager.GetSingleton().Update(Time.deltaTime);

        // call base update, remove modules
        base.Update(deltaTime);
    }

    /// <summary>
    /// Shutdown the plugin
    /// </summary>
    public override void Shutdown()
    {
        UninstallUISystem();
        UninstallSystems();
        UninstallEntitys();
    }

    /// <summary>
    /// Install NetRpc
    /// </summary>
    private void InstallNetRpc() 
    {
        object[] protocols = new object[] {
            new Rpc()
        };
        
        XNetRpc.Startup(protocols,
            new XNetInternalParser(), XRpcSessionFlag.Http);
    }

    /// <summary>
    /// Uninstall NetRpc
    /// </summary>
    private void UninstallNetRpc()
    {
        XNetRpc.Shutdown();
    }

    /// <summary>
    /// install all entity factorys
    /// </summary>
    private void InstallEntitys()
    {
        List<System.Type> autoInstallEntityTypes = new List<System.Type>();

        Assembly assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            var attrs = type.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is Initialize ii)
                {
                    if (ii.InstallType == InstallType.Startup)
                        autoInstallEntityTypes.Add(type);
                }
            }
        }

        // start entity manager, set entity data syncer
        XEntityManager.GetSingleton().Startup(autoInstallEntityTypes);
    }

    /// <summary>
    /// uninstall all entity factorys
    /// </summary>
    private void UninstallEntitys()
    {
        XEntityManager.GetSingleton().Shutdown();
    }

    /// <summary>
    /// Install product API
    /// </summary>
    /// <exception cref="System.NullReferenceException"></exception>
    private void InstallSdkAPI()
    {
        var conf = XApp.GetSingleton().GetAppConfigure();
        if (!conf)
            throw new System.NullReferenceException();

        // register product api, the app sdk will call product api to get the product info,
        // and the tracking api will call product api to get the product info when tracking events, so register product api before tracking api
        var productAPI = new InternalProductAPI(conf.versionAPI, conf.appId);
        XAppProduct.GetSingleton().RegisterAPI(productAPI);

        // tracking api depends on product api, so register it after product api
        var trackingAPI = new InternalTrackingAPI(conf);
        XAppTracking.GetSingleton().RegisterAPI(trackingAPI);

        // activity api depends on product api, so register it after product api
        var activityAPI = new InternalActivityAPI(conf);
        XAppActivity.GetSingleton().RegisterAPI(activityAPI);

        // email api depends on product api, so register it after product api
        var emailAPI = new InternalEmailAPI(conf);
        XAppEmail.GetSingleton().RegisterAPI(emailAPI);
    }

    /// <summary>
    /// Uninstall product API
    /// </summary>
    private void UninstallSdkAPI()
    {
        XAppProduct.GetSingleton().ClearAPI();
        XAppTracking.GetSingleton().ClearAPI();
        XAppActivity.GetSingleton().ClearAPI();
        XAppEmail.GetSingleton().ClearAPI();
    }

    /// <summary>
    /// install ui system
    /// </summary>
    private void InstallUISystem()
    {
        UISystem.GetSingleton().Startup(new ResourceConfigure() {
            loader = new UIResourceLoader(), cacher = new UIDefaultCacher()
        });

        UISystem.GetSingleton().RegisterSyncActions(new System.Type[] {
            typeof(SyncFancyView),
            typeof(SyncFancyJumpTo),
            typeof(SyncFancyScrollTo),
            typeof(SyncFancyClick),
            typeof(SyncFancyAnimatorView),
            typeof(SyncFancyAnimatorClick),
            typeof(SyncFancyGridScrollView),
            typeof(SyncFancyGridClick)
        });
    }

    /// <summary>
    /// uninstall ui system
    /// </summary>
    private void UninstallUISystem()
    {
        UISystem.GetSingleton().Shutdown();
    }

    /// <summary>
    /// Load configuration files asynchronously
    /// </summary>
    /// <param name="complete"></param>
    /// <returns></returns>
    private bool TryLoadAsyncConfiyFiles(System.Action complete)
    {
        var conf = XApp.GetSingleton().GetAppConfigure();
        if (!conf)
            return false;

        Assembly assembly = Assembly.GetExecutingAssembly();
        if (conf.bundleSource == AppBundleSource.Local)
        {
#if UNITY_EDITOR
            LoadAsyncConfiyDirectory<TextAsset>(conf.cfgBundle, (assets) => {
                if (ReadConfigFiles(assembly, assets))
                    complete?.Invoke();
            });
#endif
        }
        else
        {
            XRes.LoadBundleAsync(conf.cfgBundle, (bundle) => {
                var assets = bundle.LoadAllAssets();
                if (assets.Length > 0)
                {
                    if (ReadConfigFiles(assembly, assets))
                        complete?.Invoke();
                }
            });
        }

        return true;
    }

    /// <summary>
    /// Read configuration files
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="assets"></param>
    /// <returns></returns>
    private bool ReadConfigFiles(Assembly assembly, Object[] assets)
    {
        if (!TryGetExcelFields(out var fields))
            return false;

        for (int idx = 0; idx < assets.Length; idx++)
        {
            if (assets[idx] is TextAsset asset)
            {
                var text = System.Text.Encoding.UTF8.GetString(asset.bytes);
                if (!string.IsNullOrEmpty(text))
                {
                    var name = GetConfigTypeName(asset.name);

                    var type = assembly.GetType(name);
                    if (type != null)
                    {
                        IConfigData configData = (IConfigData)JsonConvert.DeserializeObject(text, type);
                        if (fields.TryGetValue(configData.GetName(), out var value))
                        {
                            var datas = configData.GetData();
                            value.SetValue(null, datas);
#if UNITY_EDITOR
                            Debug.Log($"[{Time.frameCount}] Load excel config file {configData.GetName()} done");
#endif
                        }
                    }
                }
            }
        }

        System.GC.Collect();

#if UNITY_EDITOR
        Debug.Log($"[{Time.frameCount}] Load excel config file finished");
#endif
        return true;
    }

    /// <summary>
    /// Get excel fields
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    private bool TryGetExcelFields(out Dictionary<string, FieldInfo> fields)
    {
        fields = new Dictionary<string, FieldInfo>();

        System.Type excelType = typeof(Excel);
        foreach (FieldInfo fiedlInfo in excelType.GetFields())
        {
            if (fiedlInfo.IsPublic && fiedlInfo.IsStatic && fiedlInfo.IsInitOnly)
                fields.Add(fiedlInfo.Name, fiedlInfo);
        }

        return true;
    }

    /// <summary>
    /// Get configuration type name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string GetConfigTypeName(string name)
    {
        return $"Config.{name}";
    }

#if UNITY_EDITOR
    /// <summary>
    /// From directory value configuration file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="complete"></param>
    private void LoadAsyncConfiyDirectory<T>(string path, System.Action<T[]> complete) where T : Object
    {
        if (Directory.Exists(path))
        {
            List<T> assets = new List<T>();

            var files = AssetDatabase.FindAssets("t:" + typeof(T).Name, new string[] { path });
            foreach (var file in files)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(file);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (asset)
                        assets.Add(asset);
                }
            }

            complete?.Invoke(assets.ToArray());
        }
    }
#endif

    /// <summary>
    /// Rewrite module loading to achieve RPC protocol registration
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override XEventModule Load(System.Type type)
    {
        if (!modules.TryGetValue(type.Name, out var module))
        {
            if (TryGetFunctionSystem(type, out var functionSystem))
            {
                module = (XEventModule)System.Activator.CreateInstance(type);
                module.SetDispatcher(this);

                if (module.Install())
                {
                    XNetRpc.RegisterProtocol(module);
                    if (functionSystem.Active != 0)
                        module.Active();
                }

#if UNITY_EDITOR
                Debug.Log($"[{Time.frameCount}] Load system {type.Name} done");
#endif

                modules.Add(type.Name, module);
            }
        }
        else
        {
            module.Active();
        }

        return module;
    }

    /// <summary>
    /// Rewrite module loading to achieve RPC protocol unregistration
    /// </summary>
    /// <param name="type"></param>
    public override void Unload(System.Type type)
    {
        if (modules.TryGetValue(type.Name, out XEventModule module))
        {
            module.Detive();
            module.Uninstall();

#if UNITY_EDITOR
            Debug.Log($"[{Time.frameCount}] Unload system {type.Name} done");
#endif
            XNetRpc.UnregisterProtocol(module);

            removes.Enqueue(type.Name);
        }
    }

    /// <summary>
    /// Unload all modules
    /// </summary>
    public override void UnloadAll()
    {
        foreach (var it in modules)
        {
            // unregister rpc protocol
            XNetRpc.UnregisterProtocol(it.Value);
            
            // destroy module
            it.Value.Detive();
            it.Value.Uninstall();

#if UNITY_EDITOR
            Debug.Log($"[{Time.frameCount}] Unload module {it.Key}");
#endif
        }

        modules.Clear();
    }

    /// <summary>
    /// Try get function system by type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="functionSystem"></param>
    /// <returns></returns>
    /// <exception cref="System.NullReferenceException"></exception>
    private bool TryGetFunctionSystem(System.Type type, out FunctionSystem functionSystem)
    {
        foreach (var it in Excel.functionSystemConfig)
        {
            if (it.Value.ClassType == type.Name)
            {
                functionSystem = it.Value;
                return true;
            }
        }

        throw new System.NullReferenceException($"Can't find function system {type.Name}");
    }
}
