using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Oathx.Avatar.Editor
{
    public class XAvatarInspectorWindow : EditorWindow
    {
        [MenuItem("Avatar/Avatar")]
        static void OpenEditor()
        {
            XAvatarInspectorWindow avatarWindow = GetWindow<XAvatarInspectorWindow>();
            avatarWindow.titleContent = new GUIContent("Avatar");
            avatarWindow.Show();
        }

        [MenuItem("Avatar/Check Material References")]
        static void CheckMaterialReferences()
        {
            var selectedObjects = Selection.gameObjects;
            foreach (var go in selectedObjects)
            {
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (!mat)
                            Debug.LogError($"GameObject {go.name} There are missing material references");
                    }
                }
            }
        }

        enum TabPage
        {
            Avatar,
            AnimationClip
        }

        private TabPage _pageIndex = TabPage.Avatar;
        private Dictionary<TabPage, ITabPage> _pages = new Dictionary<TabPage, ITabPage>();
        private int _fps = 0;
        private Texture2D _refreshTexture;
        public XAvatarInspectorWindow()
        {
            RegisterTabPage(TabPage.Avatar, new XAvatarTabPage(this));
            RegisterTabPage(TabPage.AnimationClip, new XAvatarAnimationClipTab(this));
        }

        private void RegisterTabPage(TabPage pageType, ITabPage page)
        {
            if (!_pages.ContainsKey(pageType))
            {
                _pages.Add(pageType, page);
            }
        }

        private void OnGUI()
        {
            Rect subPos = GetSubWindowArea();

            if (!_refreshTexture)
                _refreshTexture = EditorGUIUtility.FindTexture("Refresh");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_refreshTexture, GUILayout.Width(32)))
            {
                if (_pages.TryGetValue(_pageIndex, out var page))
                {
                    page.OnReload();
                }
            }
            _pageIndex = (TabPage)GUILayout.Toolbar((int)_pageIndex, Enum.GetNames(typeof(TabPage)));
            GUILayout.EndHorizontal();
            if (_pages.TryGetValue(_pageIndex, out var tab))
            {
                tab.OnGUI(subPos);
            }
        }

        private void OnEnable()
        {
            Rect subPos = GetSubWindowArea();
            foreach (KeyValuePair<TabPage, ITabPage> v in _pages)
            {
                v.Value.OnEnable(subPos);
            }
            
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<TabPage, ITabPage> v in _pages)
            {
                v.Value.OnDisable();
            }
        }

        private void Update()
        {
            _fps++;
            if (_fps >= 10)
            {
                Repaint();
                _fps = 0;
            }
        }

        private Rect GetSubWindowArea()
        {
            return new Rect(0, 0, position.width, position.height);
        }
    }
}
