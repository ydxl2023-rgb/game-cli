using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// Asset Tree Item
    /// </summary>
    public class AssetTreeItem : TreeViewItem
    {
        /// <summary>
        /// asset info
        /// </summary>
        public BundleAssetInfo asset
        { get; private set; }

        /// <summary>
        /// item color
        /// </summary>
        public Color itemColor
        { get; private set; }

        /// <summary>
        /// 资源真实归属的 bundle 名（来自 .meta，与处理顺序无关）。
        /// 空字符串表示该资源不属于任何 bundle。
        /// </summary>
        public string ownerBundleName
        { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="currentBundlePath">当前选中 bundle 的 bundlePath</param>
        /// <param name="currentDirectAssetPaths">当前 bundle 的直接资源路径集合（小写），
        /// 用于判断颜色：资源在该集合中显示白色（本 bundle 直接资源），否则灰色</param>
        /// <param name="assetToBundle">全局"资源路径 → 归属 bundle 名"映射，用于显示 Bundle 列</param>
        public AssetTreeItem(BundleAssetInfo assetInfo, string currentBundlePath,
            HashSet<string> currentDirectAssetPaths, Dictionary<string, string> assetToBundle)
        {
            asset = assetInfo;

            // initialize
            Init(currentBundlePath, currentDirectAssetPaths, assetToBundle);
        }

        /// <summary>
        /// initialize
        /// </summary>
        private void Init(string currentBundlePath, HashSet<string> currentDirectAssetPaths,
            Dictionary<string, string> assetToBundle)
        {
            id = Animator.StringToHash(asset.path);
            displayName = Path.GetFileName(asset.path);
            icon = AssetDatabase.GetCachedIcon(asset.path) as Texture2D;

            // 从预建映射查真实归属 bundle 名（基于 Unity 真实归属，与处理顺序无关）
            if (assetToBundle != null && assetToBundle.TryGetValue(asset.path, out string owner))
            {
                ownerBundleName = owner;
            }
            else
            {
                ownerBundleName = string.Empty;
            }

            // 颜色语义：是否是"当前选中 bundle"的直接资源。
            // 优先用预取的直接资源集合判断（最可靠），回退到归属 bundle 名比较。
            bool isDirectOfCurrent = currentDirectAssetPaths != null
                && currentDirectAssetPaths.Contains(asset.path);
            if (!isDirectOfCurrent && !string.IsNullOrEmpty(ownerBundleName))
            {
                isDirectOfCurrent = string.Equals(ownerBundleName, currentBundlePath,
                    System.StringComparison.OrdinalIgnoreCase);
            }
            itemColor = isDirectOfCurrent ? Color.white : Color.gray;
        }
    }

    /// <summary>
    /// asset selected handler
    /// </summary>
    /// <param name="assetItem"></param>

    public delegate void AssetSelectedHandler(AssetTreeItem assetItem);

    public enum BundleAssetTreeSortOption
    {
        Asset,
        Bundle,
        Size,
        Message
    }

    /// <summary>
    /// Asset Tree View
    /// </summary>
    public class AssetTreeView : TreeView
    {
        /// <summary>
        /// asset selected event
        /// </summary>
        public event AssetSelectedHandler OnAssetSelected;

        /// <summary>
        /// bundles
        /// </summary>
        private List<BundleNode> bundles = new List<BundleNode>();

        /// <summary>
        /// sort options
        /// </summary>
        private BundleAssetTreeSortOption[] sortOptions = {
            BundleAssetTreeSortOption.Asset,
            BundleAssetTreeSortOption.Bundle,
            BundleAssetTreeSortOption.Size,
            BundleAssetTreeSortOption.Message
        };

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="mchState"></param>
        public AssetTreeView(TreeViewState state, MultiColumnHeaderState mchState)
            : base(state, new MultiColumnHeader(mchState))
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }

        /// <summary>
        /// sorting changed
        /// </summary>
        /// <param name="multiColumnHeader"></param>
        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        private bool SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return false;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return false;

            AssetSortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();

            return true;
        }

        /// <summary>
        /// sort by column
        /// </summary>
        /// <returns></returns>
        private bool AssetSortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return false;

            List<AssetTreeItem> assetList = new List<AssetTreeItem>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as AssetTreeItem);
            }

            var orderedItems = InitialOrder(assetList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();

            return true;
        }

        /// <summary>
        /// initial order
        /// </summary>
        /// <param name="myTypes"></param>
        /// <param name="columnList"></param>
        /// <returns></returns>
        private IOrderedEnumerable<AssetTreeItem> InitialOrder(IEnumerable<AssetTreeItem> myTypes, int[] columnList)
        {
            BundleAssetTreeSortOption sortOption = sortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case BundleAssetTreeSortOption.Asset:
                    return myTypes.Order(l => l.displayName, ascending);
                case BundleAssetTreeSortOption.Size:
                    return myTypes.Order(l => l.asset.size, ascending);
                
                default:
                    // 按"是否当前 bundle 的直接资源"排序（白色行排前），与颜色语义一致
                    return myTypes.Order(l => l.itemColor == Color.white, ascending);
            }
        }

        /// <summary>
        /// refresh data
        /// </summary>
        /// <param name="nodes"></param>
        public void Refresh(List<BundleNode> nodes)
        {
            bundles = nodes;

            if (bundles.Count != 0)
                Reload();
        }

        /// <summary>
        /// build root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem
            {
                id = 0,
                depth = -1,
                displayName = "Root"
            };

            // create asset dependencies items
            // 预建"资源路径 → 归属 bundle 名"全局映射（基于 Unity 真实归属，与处理顺序无关）。
            // 同时为当前 bundle 预取直接资源集合。二者都用 AssetDatabase 查询而非 AssetImporter，
            // 避免路径大小写/格式差异导致查不到归属。
            Dictionary<string, string> assetToBundle = BuildAssetToBundleMap();

            List<TreeViewItem> assetItems = new List<TreeViewItem>();
            foreach (var bundle in bundles)
            {
                // 当前 bundle 的直接资源路径集合（小写），用于判断颜色
                HashSet<string> directAssetPaths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var p in AssetDatabase.GetAssetPathsFromAssetBundle(bundle.bundlePath))
                {
                    directAssetPaths.Add(p.ToLower());
                }

                foreach (var asset in bundle.dependencies)
                {
                    assetItems.Add(new AssetTreeItem(asset, bundle.bundlePath, directAssetPaths, assetToBundle));
                }
            }

            root.children = assetItems;

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        /// <summary>
        /// 构建"资源路径（小写）→ 归属 bundle 名"全局映射。
        /// 遍历所有 bundle，对每个 bundle 查其直接资源，反向建立映射。
        /// 基于 Unity 真实归属（GetAssetPathsFromAssetBundle），与依赖收集的处理顺序无关。
        /// </summary>
        private static Dictionary<string, string> BuildAssetToBundleMap()
        {
            var map = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            foreach (var bundleName in allBundles)
            {
                foreach (var assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
                {
                    map[assetPath.ToLower()] = bundleName;
                }
            }
            return map;
        }

        /// <summary>
        /// row GUI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as AssetTreeItem, args.GetColumn(i), ref args);
        }

        /// <summary>
        /// cell GUI
        /// </summary>
        /// <param name="cellRect"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        /// <param name="args"></param>
        private bool CellGUI(Rect cellRect, AssetTreeItem item, int column, ref RowGUIArgs args)
        {
            BundleAssetInfo assetInfo = item.asset;
            if (!assetInfo)
                return false;

            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            if (column != 3)
                GUI.color = item.itemColor;

            MyColumns myColumns = (MyColumns)column;
            switch (myColumns)
            {
                case MyColumns.Message:
                    if (assetInfo.state != ErrorState.None)
                    {
                        Texture icon = EditorGUIUtility.FindTexture("console.warnicon");
                        if (icon)
                        {
                            var iconRect = new Rect(cellRect.x, cellRect.y, cellRect.height, cellRect.height);
                            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                        }
                    }
                    break;

                case MyColumns.Asset:
                    {
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if (item.icon != null)
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);

                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height),
                            item.displayName,
                            args.selected,
                            args.focused);
                    }
                    break;

                case MyColumns.Size:
                    DefaultGUI.Label(cellRect, item.asset.GetFileSize(), args.selected, args.focused);
                    break;

                case MyColumns.Bundle:
                    // 显示资源真实归属 bundle（来自 .meta），与处理顺序无关
                    if (!string.IsNullOrEmpty(item.ownerBundleName))
                        DefaultGUI.Label(cellRect, item.ownerBundleName, args.selected, args.focused);
                    break;
            }

            GUI.color = oldColor;

            return true;
        }

        /// <summary>
        /// selection changed
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count > 0)
            {
                var item = FindItem(selectedIds[0], rootItem) as AssetTreeItem;
                if (item != null)
                {
                    Object target = AssetDatabase.LoadAssetAtPath<Object>(item.asset.path);
                    if (target)
                    {
                        Selection.activeObject = target;
                    }

                    BundleMessage.GetSingleton().SetMessage(item.asset);

                    OnAssetSelected?.Invoke(item);
                }
            }
        }

        /// <summary>
        /// context clicked
        /// </summary>
        protected override void ContextClicked()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Asset"), false, AddAsset);
            menu.AddItem(new GUIContent("Remove Asset"), false, RemoveSelectedAsset);
            menu.AddItem(new GUIContent("Select in Project"), false, SelectInProject);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("View Dependencies"), false, ViewDependencies);
            menu.ShowAsContext();
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddAsset() { }

        /// <summary>
        /// 
        /// </summary>
        private void RemoveSelectedAsset() { }

        /// <summary>
        /// 
        /// </summary>
        private void SelectInProject() { }

        /// <summary>
        /// 
        /// </summary>
        private void ViewDependencies() { }

        /// <summary>
        /// create default multi column header state
        /// </summary>
        /// <returns></returns>
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        /// <summary>
        /// get columns
        /// </summary>
        /// <returns></returns>
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };

            retVal[0].headerContent = new GUIContent("!", "Errors, Warnings, or Info");
            retVal[0].minWidth = 16;
            retVal[0].width = 16;
            retVal[0].maxWidth = 16;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = false;

            retVal[1].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[1].minWidth = 50;
            retVal[1].width = 150;
            retVal[1].maxWidth = 300;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Size", "Size on disk");
            retVal[2].minWidth = 30;
            retVal[2].width = 75;
            retVal[2].maxWidth = 100;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("Bundle", "Bundle name. 'auto' means asset was pulled in due to dependency");
            retVal[3].minWidth = 50;
            retVal[3].width = 100;
            retVal[3].maxWidth = 300;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = true;

            return retVal;
        }

        /// <summary>
        /// columns
        /// </summary>
        public enum MyColumns
        {
            Message,
            Asset,
            Size,
            Bundle
        }
    }
}