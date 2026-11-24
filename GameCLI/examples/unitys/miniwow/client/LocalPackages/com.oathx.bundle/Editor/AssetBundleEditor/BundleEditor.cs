using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBundleEditor
    {
        /// <summary>
        /// enable window
        /// </summary>
        /// <param name="rect"></param>
        void OnEnable(Rect rect);

        /// <summary>
        /// disable window
        /// </summary>
        void OnDisable();

        /// <summary>
        /// override gui
        /// </summary>
        /// <param name="rect"></param>
        void OnGUI(Rect rect);

        /// <summary>
        /// on reload
        /// </summary>
        void OnReload();

        /// <summary>
        /// on update
        /// </summary>
        void OnUpdate();
    }

    /// <summary>
    /// window mode
    /// </summary>
    public enum BundleWindowMode
    {
        /// <summary>
        /// configure mode
        /// </summary>
        Configure,
        /// <summary>
        /// build mode
        /// </summary>
        Build,
        /// <summary>
        /// track mode
        /// </summary>
        Track
    }

    /// <summary>
    /// new asset bundle editor
    /// </summary>
    public class BundleEditor : EditorWindow
    {
        /// <summary>
        /// eidt windows
        /// </summary>
        private readonly Dictionary<BundleWindowMode, IBundleEditor> windows = new Dictionary<BundleWindowMode, IBundleEditor>();
        private Texture2D refreshTexture;

        /// <summary>
        /// current window mode
        /// </summary>
        private BundleWindowMode currentMode = BundleWindowMode.Configure;

        /// <summary>
        /// open asset bundle editor
        /// </summary>
        [MenuItem("AssetBundle/Open")]
        static void OpenAssetBundleEditor()
        {
            BundleEditor window = GetWindow<BundleEditor>();
            window.titleContent = new GUIContent(typeof(AssetBundle).Name);
            window.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public BundleEditor()
        {
            windows.Add(BundleWindowMode.Configure, new BundleConfigureEditor(this));
            windows.Add(BundleWindowMode.Build, new BundleBuilderEditor(this));
            windows.Add(BundleWindowMode.Track, new BundleAnalysisEditor(this));
        }

        /// <summary>
        /// get sub window area
        /// </summary>
        /// <returns></returns>
        private Rect GetSubWindowArea()
        {
            return new Rect(0, 32, position.width, position.height);
        }

        /// <summary>
        /// get sub window area
        /// </summary>
        public void OnEnable() 
        {
            refreshTexture = EditorGUIUtility.FindTexture("Refresh");

            foreach (var tab in windows)
            {
                tab.Value.OnEnable(GetSubWindowArea());
            }
        }

        /// <summary>
        /// disable window
        /// </summary>
        public void OnDisable()
        {
            foreach (var tab in windows)
            {
                tab.Value.OnDisable();
            }
        }

        /// <summary>
        /// GUI
        /// </summary>
        public void OnGUI()
        {
            Rect rect = GetSubWindowArea();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(refreshTexture, GUILayout.Width(30)))
            {
                //BundleUtility.RefreshBundles();

                if (windows.TryGetValue(currentMode, out var table))
                {
                    if (table is BundleConfigureEditor v)
                        v.OnReload();
                }
            }

            string[] modes = System.Enum.GetNames(typeof(BundleWindowMode));
            currentMode = (BundleWindowMode)GUILayout.Toolbar((int)currentMode, modes);
            GUILayout.EndHorizontal();

            if (windows.TryGetValue(currentMode, out var tab))
            {
                tab.OnGUI(rect);
            }
        }

        /// <summary>
        /// fps count
        /// </summary>
        private int fps = 0;

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (windows.TryGetValue(currentMode, out var tab))
                tab.OnUpdate();

            fps++;
            if (fps >= 10)
            {
                Repaint();
                fps = 0;
            }
        }
    }
}
