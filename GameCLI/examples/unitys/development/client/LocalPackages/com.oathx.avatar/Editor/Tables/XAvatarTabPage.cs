using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Animations;

namespace Oathx.Avatar.Editor
{
    public interface ITabPage
    {
        void OnEnable(Rect rect);
        void OnDisable();
        void OnGUI(Rect rect);
        void OnReload();
    }

    public class XAvatarTabPage : ITabPage
    {
        internal class ToggleProperty
        {
            public ToggleProperty(bool value, string title, string tooltip,
                List<string> onToggles, string reserve = "", BuildAssetBundleOptions opt = BuildAssetBundleOptions.None)
            {
                if (onToggles.Contains(title))
                    state = true;
                else
                    state = value;

                content = new GUIContent(title, tooltip);
                option = opt;
                text = reserve;
            }

            public bool state;
            public GUIContent content;
            public BuildAssetBundleOptions option;
            public string text;
        }

        private XAvatarTreeView _treeView;
        private TreeViewState _treeViewState;
        private MultiColumnHeaderState _multiColumnHeaderState;
        private Rect _position;
        private List<XAvatarInfo> _avatars = new List<XAvatarInfo>();
      
        private TreeViewState _partViewState;
        private MultiColumnHeaderState _partMCHState;
        private XAvatarPartTreeView _partTreeView;

        public XAvatarTabPage(EditorWindow window)
        {
           
        }

        public void OnEnable(Rect rect)
        {
            var complites = XAvatarEditorSetting.Instance.complites;
            foreach(var c in complites)
            {
                Refresh(c);
            }

            XAvatarEditorSetting.Save();
        }

        public void OnDisable()
        {
  
        }

        public void OnGUI(Rect rect)
        {
            _position = rect;
           
            InitIfNeeded();

            if (_treeView != null)
                _treeView.OnGUI(multiColumnTreeViewRect);

            InitIfNeedPartTreeView();
            if (_partTreeView != null)
                _partTreeView.OnGUI(partColumnTreeViewRect);

            GUILayout.BeginArea(buttonRect);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                foreach (var avatar in _avatars)
                {
                    avatar.status = true;
                }
            }
            if (GUILayout.Button("Deselect All"))
            {
                foreach (var avatar in _avatars)
                {
                    avatar.status = false;
                }
            }

            if (GUILayout.Button("Build Avatars"))
            {
                BuildAvatars();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void BuildAvatars()
        {
            foreach (var avatar in _avatars)
            {
                if (avatar.status)
                {
                    var fullAssetPath = AssetDatabase.GetAssetPath(avatar.prefab);
                    GenerateAvatar(fullAssetPath, avatar.animatorController, avatar.outpath);
                }
            }
        }

        private bool GenerateAvatar(string path, AnimatorController animatorController, string outDirectory)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab)
                return false;

            GameObject srcfbx = AssetDatabase.LoadAssetAtPath<GameObject>(Path.ChangeExtension(path,
               XAvatarUtility.InternalDirectoy.Fbx.ToString().ToLower()));
            if (!srcfbx)
                return false;

            // AnimatorController 允许为空：ExtractSkeleton 内部已判空，空时跳过 override controller 生成
            XAvatarUtility.ExtractSkeleton(prefab, srcfbx, path, animatorController, outDirectory);
            XAvatarUtility.ExtractSkinMesh(prefab, srcfbx, path, outDirectory);
            return true;
        }

        public void SelectAvatarPrefab(IEnumerable<XAvatarElement> elements)
        {
            if (_partTreeView != null)
                _partTreeView.SetAvatarPrefab(elements);
        }

        private void InitIfNeeded()
        {
            if (_treeView == null) 
            {
                if (_treeViewState == null)
                    _treeViewState = new TreeViewState();

                bool firstInit = _multiColumnHeaderState == null;
                var headerState = XAvatarTreeView.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);

                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);

                _multiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                _treeView = new XAvatarTreeView(_treeViewState, multiColumnHeader, this);
                _treeView.Reload(); 
            }
        }

        private void InitIfNeedPartTreeView()
        {
            if (_partTreeView == null)
            {
                Refresh();

                if (_partViewState == null)
                    _partViewState = new TreeViewState();

                var headerState = XAvatarPartTreeView.CreateDefaultMultiColumnHeaderState();
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_partMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_partMCHState, headerState);

                _partMCHState = headerState;
                _partTreeView = new XAvatarPartTreeView(_partViewState, _partMCHState);
                _partTreeView.Reload();
            }
        }

        protected Rect multiColumnTreeViewRect
        {
            get { return new Rect(5, 28, _position.width - 5, _position.height * 0.3f); }
        }

        protected Rect partColumnTreeViewRect
        {
            get { return new Rect(5, 10 + _position.height * 0.322f, _position.width - 10, _position.height * 0.62f); }
        }

        protected Rect buttonRect
        {
            get { return new Rect(5, 16 + _position.height * 0.95f, _position.width - 10, _position.height * 0.05f); }
        }

        public void OnReload()
        {
            Refresh();
        }

        private void Refresh()
        {
            _avatars.Clear();

            var complites = XAvatarEditorSetting.Instance.complites;
            foreach (var c in complites)
            {
                Refresh(c);
            }

            XAvatarEditorSetting.Save();

            if (_treeView != null)
            {
                _treeView.SetAvatars(_avatars);
                _treeView.Reload();
            }
        }

        private bool Refresh(XAvatarComplite compliteInfo)
        {
            // AnimatorController 允许为空：仅表示该目录的角色不生成 override controller
            if (!XAvatarUtility.TryFindAssets<GameObject>(out var results))
                return false;

            foreach (var resultGo in results)
            {
                string path = AssetDatabase.GetAssetPath(resultGo);
                if (path.Contains(compliteInfo.inputDirectory) && path.Contains(".prefab"))
                {
                    var prefabIcon = AssetDatabase.GetCachedIcon(path) as Texture2D;

                    var animatorControllerIcon = compliteInfo.animatorController
                        ? AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(compliteInfo.animatorController)) as Texture2D
                        : null;

                    XAvatarInfo info = new XAvatarInfo()
                    {
                        animatorController = compliteInfo.animatorController,
                        prefab = resultGo,
                        outpath = compliteInfo.outDirectory,
                        status = true,
                        prefabIcon = prefabIcon,
                        animatorControllerIcon = animatorControllerIcon
                    };

                    _avatars.Add(info);
                }
            }

            return true;
        }
    }
}
