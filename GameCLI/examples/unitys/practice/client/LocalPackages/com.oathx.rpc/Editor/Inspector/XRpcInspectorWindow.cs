using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Oathx.Rpc;
using UnityEditor.Callbacks;
using System.IO;
using System.Reflection;
using System;
using System.Linq;

namespace Oathx.Rpc.Editor
{
    interface XRpcTrackTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        void OnEnable(Rect rect);

        /// <summary>
        /// 
        /// </summary>
        void OnDisable();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        void OnGUI(Rect rect);

        /// <summary>
        /// 
        /// </summary>
        void OnReload();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        void OnOpened(XRpcTrackInfo track);

        /// <summary>
        /// 
        /// </summary>
        void OnUpdate();
    }

    public class XRpcInspectorWindow : EditorWindow
    {
        [MenuItem("Rpc/Track")]
        static void OpenEditor()
        {
            XRpcInspectorWindow window = GetWindow<XRpcInspectorWindow>();
            window.titleContent = new GUIContent("Rpc");
            window.Show();
        }

        /// <summary>
        /// rpc configrue
        /// </summary>
        private int _rpcSessionIndex = 0;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<XRpcType, XRpcTrackTable> 
            _rpcTabs = new Dictionary<XRpcType, XRpcTrackTable>();
        
        /// <summary>
        /// 
        /// </summary>
        private readonly List<string> _rpcNames = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        private readonly List<XRpcType> _rpcTypes = new List<XRpcType>();

        /// <summary>
        /// 
        /// </summary>
        private XRpcTrackLog _rpcTrack = new XRpcTrackLog();

        /// <summary>
        /// 
        /// </summary>
        public XRpcInspectorWindow()
        {
            _rpcTabs.Add(XRpcType.Http, new XRpcHttpTable());
            _rpcTabs.Add(XRpcType.Tcp, new XRpcTcpTable());
            _rpcTabs.Add(XRpcType.Web, new XRpcWebTable());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Rect GetSubWindowArea()
        {
            return new Rect(0, 0, position.width, position.height);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            CollectAllRpcMethods();

            Rect subPos = GetSubWindowArea();
            foreach (var track in _rpcTrack.tracks)
            {
                if (_rpcTabs.TryGetValue(track.rpcType, out var table))
                {
                    table.OnOpened(track);
                    table.OnEnable(subPos);
                }

                if (!_rpcTypes.Contains(track.rpcType))
                    _rpcTypes.Add(track.rpcType);

                var rpcTypeName = track.rpcType.ToString();
                if (!_rpcNames.Contains(rpcTypeName))
                    _rpcNames.Add(rpcTypeName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            foreach (KeyValuePair<XRpcType, XRpcTrackTable> v in _rpcTabs)
            {
                v.Value.OnDisable();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            Rect subPos = GetSubWindowArea();

            _rpcSessionIndex = GUILayout.Toolbar(_rpcSessionIndex, _rpcNames.ToArray());
            if (_rpcSessionIndex >= 0 && _rpcSessionIndex < _rpcTypes.Count)
            {
                if (_rpcTabs.TryGetValue(_rpcTypes[_rpcSessionIndex], out var tab))
                {
                    tab.OnGUI(subPos);
                }
            }
        }

        private int fps = 0;
        
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (_rpcSessionIndex >= 0 && _rpcSessionIndex < _rpcTypes.Count)
            {
                if (_rpcTabs.TryGetValue(_rpcTypes[_rpcSessionIndex], out var tab))
                    tab.OnUpdate();
            }
         
            fps++;
            if (fps >= 10)
            {
                Repaint();
                fps = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblys"></param>
        /// <returns></returns>
        public bool TryGetAssemblys(out List<Assembly> assemblys)
        {
            assemblys = new List<Assembly>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Length <= 0)
                return false;

            var assemblyNames = XNetRpcSettings.Instance.injectAssemblyNames;
            if (assemblyNames == null || assemblyNames.Length <= 0)
            {
                Debug.LogError("Can't find rpc inject assembly, see Edit/Rpc/Setting/Inject Assembly Names");
                return false;
            }

            foreach (string name in assemblyNames)
            {
                var assembly = assemblies.First(v => v.GetName().Name.Equals(name));
                if (assembly != null)
                {
                    assemblys.Add(assembly);
                }
                else
                {
                    Debug.LogError($"Can't find assembly {name}");
                }
            }

            var definitions = XNetRpcSettings.Instance.assemblyDefinitionAssets;
            foreach (var def in definitions)
            {
                var assembly = assemblies.First(v => v.GetName().Name.Contains(def.name));
                if (assembly != null)
                    assemblys.Add(assembly);
            }

            return assemblys.Count > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        private void CollectMethods(Assembly assembly)
        {
            HashSet<XRpcType> types = new HashSet<XRpcType>();
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    var clientCallServer = method.GetCustomAttribute<RpcClientCallServer>();
                    if (clientCallServer != null)
                    {
                        int hash = clientCallServer.HashCode;
                        if (hash == 0)
                            hash = XNetUtility.HashString(method.Name);

                        if (_rpcTrack.TryAddMethod(clientCallServer.SessionType, method.Name, hash, 0, out var trackInfo))
                        {
                            if (!types.Contains(clientCallServer.SessionType))
                                types.Add(clientCallServer.SessionType);
                        }
                    }
                }
            }

            var names = Enum.GetNames(typeof(RpcInternal));
            foreach (var name in names)
            {
                foreach (var type in types)
                {
                    _rpcTrack.TryAddMethod(type, name, XNetUtility.HashString(name), 1, out var trackInfo);
                }
            }

            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    var serverCallClient = method.GetCustomAttribute<RpcServerCallClient>();
                    if (serverCallClient != null)
                    {
                        if (_rpcTrack.TryGetRpcTrackInfo(serverCallClient.sessionType, out var trackInfo))
                        {
                            if (trackInfo.TryGetMethod(method.Name, out var editorMethod))
                            {
                                editorMethod.serverMethods.Add($"{method.DeclaringType.Name}.{method.Name}");
                            }
                        }
                    }
                }
            }
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="rpcType"></param>
        /// <param name="name"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        private bool TryCollectServerMethods(Assembly assembly, XRpcType rpcType, string name, out List<MethodInfo> methods)
        {
            methods = new List<MethodInfo>();
            foreach(var type in assembly.GetTypes())
            {
                foreach(var method in type.GetMethods())
                {
                    var serverCallClient = method.GetCustomAttribute<RpcServerCallClient>();
                    if (serverCallClient != null && method.Name == name)
                    {
                        methods.Add(method);
                    }
                }
            }

            return methods.Count > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CollectAllRpcMethods()
        {
            if (TryGetAssemblys(out var assemblys))
            {
                foreach (var v in _rpcTrack.tracks)
                {
                    v.Reset();
                }

                foreach (var assembly in assemblys)
                {
                    CollectMethods(assembly);
                }
            }

            return true;
        }
    }
}

