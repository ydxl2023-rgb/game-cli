using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Oathx.Avatar.Editor
{
    public class XAvatarInfo
    {
        public Texture2D prefabIcon;
        public Texture2D animatorControllerIcon;

        public GameObject prefab;
        public AnimatorController animatorController;
        public string outpath;
        public bool status;
    }

    [Serializable]
    public class XAvatarElement : TreeViewItem
    {
        public XAvatarInfo info
        { get; set; }

        public XAvatarElement(int id, int depth, string name, XAvatarInfo avatarInfo)
            : base(id, depth, name)
        {
            info = avatarInfo;
            children = new List<TreeViewItem>();
        }
    }

    public class XAvatarTreeView : TreeView
    {
        const float kRowHeights = 20f;
        
        enum PrefabColumns
		{
            Status,
            Prefab,
            AnimatorController,
            Path
        }

        protected XAvatarTabPage _avatarTabPage;
        protected List<XAvatarInfo> _avatars = new List<XAvatarInfo>();

        public XAvatarTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, XAvatarTabPage tabPage)
            : base(state, multicolumnHeader)
        {
            rowHeight = kRowHeights;
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            _avatarTabPage = tabPage;

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new XAvatarElement(0, -1, "", new XAvatarInfo());
            for(int idx=0; idx<_avatars.Count; idx++)
            {
                var info = _avatars[idx];

                XAvatarElement child = new XAvatarElement(info.GetHashCode(), root.depth + 1, info.prefab.name, info);
                root.AddChild(child);
            }

            return root;
        }

        public void SetAvatars(List<XAvatarInfo> avatars)
        {
            _avatars = avatars;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var selectAvatars = new List<XAvatarElement>();
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var item = FindItem(id, rootItem) as XAvatarElement;
                    if (item != null)
                    {
                        selectAvatars.Add(item);
                        break;
                    }
                }
            }

            if (selectAvatars.Count != 0)
                _avatarTabPage.SelectAvatarPrefab(selectAvatars);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (XAvatarElement)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (PrefabColumns)args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, XAvatarElement item, PrefabColumns column, ref RowGUIArgs args)
        {
            switch(column)
            {
                case PrefabColumns.Status:
                    var statusRect = new Rect(cellRect.x + 6, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    var status = GUI.Toggle(statusRect, item.info.status, ""); 
                    if (status != item.info.status)
                    {
                        item.info.status = status;
                    }          
                    break;

                case PrefabColumns.Prefab:
                    if (item.info.prefab)
                    {
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if (item.info.prefabIcon != null)
                            GUI.DrawTexture(iconRect, item.info.prefabIcon, ScaleMode.ScaleToFit);
                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.width + 1, cellRect.y + 2, cellRect.width - iconRect.width, cellRect.height),
                            item.displayName,
                            args.selected,
                            args.focused);
                    }
                    else
                    {
                        if (_avatarTabPage != null)
                            _avatarTabPage.OnReload();
                    }
                    break;

                case PrefabColumns.AnimatorController:
                    if (item.info.animatorController)
                    {
                        var animatorControllerIconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if (item.info.animatorControllerIcon != null)
                            GUI.DrawTexture(animatorControllerIconRect, item.info.animatorControllerIcon, ScaleMode.ScaleToFit);

                        DefaultGUI.Label(
                            new Rect(cellRect.x + animatorControllerIconRect.width, cellRect.y + 2, cellRect.width - animatorControllerIconRect.width, cellRect.height),
                            item.info.animatorController.name,
                            args.selected,
                            args.focused);
                    }
                    break;
                case PrefabColumns.Path:
                    if (item.info.prefab)
                        GUI.Label(cellRect, AssetDatabase.GetAssetPath(item.info.prefab));
                    break;
            }
        }

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
		{
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(""),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 32,
                    minWidth = 32,
                    maxWidth = 32,
                    autoResize = false
                },

                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Prefab"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 120,
                    minWidth = 60,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("AnimatorController"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Left,
					width = 150,
					minWidth = 60,
					autoResize = true
				},
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Path"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 200,
                    minWidth = 60,
                    autoResize = true
                },
            };

			var state = new MultiColumnHeaderState(columns);
			return state;
		}
	}
}
