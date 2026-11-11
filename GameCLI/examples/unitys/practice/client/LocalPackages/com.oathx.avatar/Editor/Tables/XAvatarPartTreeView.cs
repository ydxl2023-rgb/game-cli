using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;

namespace Oathx.Avatar.Editor
{
    public class XAvatarPartInfo
    {
        public string name;
        public UnityEngine.Object partObject;
        public Material material;
        public string typeName;
        public GameObject prefab;
    }

    public class XAvatarPartElement : TreeViewItem
    {
        public XAvatarPartInfo info
        { get; set; }

        public XAvatarPartElement(int id, int depth, XAvatarPartInfo avatarPartInfo)
            : base(id, depth, avatarPartInfo != null ? avatarPartInfo.name : "")
        {
            children = new List<TreeViewItem>();
            info = avatarPartInfo;
        }
    }

    public class XAvatarPartTreeView : TreeView
    {
        private XAvatarElement _element;

        enum MyColumns
        {
            Name,
            Type,
            Mesh,
            Material
        }

        public XAvatarPartTreeView(TreeViewState state, MultiColumnHeaderState mchs)
            : base(state, new MultiColumnHeader(mchs))
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new XAvatarPartElement(0, -1, new XAvatarPartInfo());
            if (_element != null)
            {
                XAvatarInfo info = _element.info;
                if (info.prefab)
                {
                    SkinnedMeshRenderer[] smrs = info.prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach(var smr in smrs)
                    {
                        var part = new XAvatarPartElement(smr.name.GetHashCode(), root.depth + 1, new XAvatarPartInfo() {
                            partObject = smr, material = smr.sharedMaterial, name=smr.name, typeName=typeof(SkinnedMeshRenderer).Name, prefab = info.prefab
                        });
                        root.AddChild(part);
                    }

                    MeshRenderer[] mrs = info.prefab.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var mr in mrs)
                    {
                        var part = new XAvatarPartElement(mr.name.GetHashCode(), root.depth + 1, new XAvatarPartInfo()
                        {
                            partObject = mr,
                            material = mr.sharedMaterial,
                            name = mr.name,
                            typeName = typeof(MeshRenderer).Name,
                            prefab = info.prefab
                        });
                        root.AddChild(part);
                    }
                }
            }
            return root;
        }

        public void SetAvatarPrefab(IEnumerable<XAvatarElement> elements)
        {
            if (elements.Count() > 0)
            {
                _element = elements.First();
                Reload();
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as XAvatarPartElement, args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, XAvatarPartElement item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);

            GUI.color = new Color(0.7f, 0.7f,0.7f, 1f);
            MyColumns myColumns = (MyColumns)column;
            switch (myColumns)
            {
                case MyColumns.Name:
                    {
                        DefaultGUI.Label(
                            cellRect,
                            item.displayName,
                            args.selected,
                            args.focused);
                    }
                    break;
                case MyColumns.Type:
                    if (item.info != null)
                        DefaultGUI.Label(cellRect, item.info.typeName, args.selected, args.focused);
                    break;
                case MyColumns.Mesh:
                    if (item.info != null)
                    {
                        var nameRect = cellRect;
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if (item.info.partObject)
                        {
                            var asset = AssetDatabase.GetAssetPath(item.info.prefab);
                            if (!string.IsNullOrEmpty(asset))
                            {
                                var icon = AssetDatabase.GetCachedIcon(asset) as Texture2D;
                                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                                nameRect = new Rect(cellRect.x + iconRect.width,
                                    cellRect.y, cellRect.width, cellRect.height);
                            }

                            DefaultGUI.Label(nameRect, item.info.partObject.name, args.selected, args.focused);
                        }
                    }
                        
                    break;
                case MyColumns.Material:
                    if (item.info != null)
                    {
                        var nameRect = cellRect;
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if (item.info.material)
                        {
                            var asset = AssetDatabase.GetAssetPath(item.info.material);
                            if (!string.IsNullOrEmpty(asset))
                            {
                                var icon = AssetDatabase.GetCachedIcon(asset) as Texture2D;
                                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                                nameRect = new Rect(cellRect.x + iconRect.width,
                                    cellRect.y, cellRect.width, cellRect.height);
                            }

                            DefaultGUI.Label(nameRect, item.info.material.name, args.selected, args.focused);
                        }
                    }
                        
                    break;
            }
            GUI.color = oldColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Name"),
                    minWidth = 30,
                    width = 70,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column(){
                    headerContent = new GUIContent("Type"),
                    minWidth = 30,
                    width = 150,
                    maxWidth = 200,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Mesh"),
                    minWidth = 30,
                    width = 100,
                    maxWidth = 150,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Material"),
                    minWidth = 30,
                    width = 100,
                    maxWidth = 200,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }
    }
}
