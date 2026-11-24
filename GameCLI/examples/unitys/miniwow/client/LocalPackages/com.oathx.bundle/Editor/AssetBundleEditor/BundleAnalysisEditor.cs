using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using Oathx.Bundle.Assets;
using System.IO;
using System.Text;
using UnityEditorInternal;

namespace Oathx.Bundle.Editor
{

    /// <summary>
    /// Bundle List Tree Item
    /// </summary>
    public class BundleListTreeItem : TreeViewItem
    {
        /// <summary>
        /// bundle info
        /// </summary>
        public XBundle bundle
        { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="bundleNode"></param>
        public BundleListTreeItem(XBundle bundleInfo)
            : base(bundleInfo.path.GetHashCode(), 0, Path.GetFileNameWithoutExtension(bundleInfo.path))
        {
            bundle = bundleInfo;
        }
    }

    /// <summary>
    /// Bundle List Tree View
    /// </summary>
    public class BundleListTreeView : TreeView
    {
        /// <summary>
        /// bundle selected handler
        /// </summary>
        public delegate void BundleSelectedHandler(BundleListTreeItem bundleItem);
        /// <summary>
        /// on bundle selected
        /// </summary>
        public event BundleSelectedHandler OnBundleSelected;

        /// <summary>
        /// sort options
        /// </summary>
        public enum BundleSortOption
        {
            Name,
            Type,
            State,
            Ref,
            Max,
            Size,
            Time,
            Path
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="mchState"></param>
        public BundleListTreeView(TreeViewState state, MultiColumnHeaderState mchState)
            : base(state, new MultiColumnHeader(mchState))
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            Reload();
            multiColumnHeader.SetSorting((int)BundleSortOption.Size, false);
        }

        public void ApplyCurrentSort()
        {
            SortIfNeeded(rootItem, GetRows());
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
        /// sort if needed
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

            SortByColumn();

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
        private bool SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return false;

            List<BundleListTreeItem> bundleList = new List<BundleListTreeItem>();
            foreach (var item in rootItem.children)
            {
                bundleList.Add(item as BundleListTreeItem);
            }

            var orderedItems = InitialOrder(bundleList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
            return true;
        }

        /// <summary>
        /// initial order
        /// </summary>
        /// <param name="items"></param>
        /// <param name="columnList"></param>
        /// <returns></returns>
        private IOrderedEnumerable<BundleListTreeItem> InitialOrder(IEnumerable<BundleListTreeItem> items, int[] columnList)
        {
            BundleSortOption sortOption = (BundleSortOption)columnList[0];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            var orderedItems = items.OrderBy(l =>
                BundleDisplayStateResolver.GetSortPriority(GetDisplayState(l.bundle)));

            switch (sortOption)
            {
                case BundleSortOption.Name:
                    return orderedItems.ThenBy(l => l.bundle.entry.name, ascending);
                case BundleSortOption.Type:                    
                    return orderedItems.ThenBy(l => l.bundle.entry.locationType, ascending);
                case BundleSortOption.State:
                    return orderedItems.ThenBy(l => GetDisplayState(l.bundle), ascending);
                case BundleSortOption.Ref:
                    return orderedItems.ThenBy(l => l.bundle.refCount, ascending);
                case BundleSortOption.Max:
                    return orderedItems.ThenBy(l => l.bundle.maxRefCount, ascending);
                case BundleSortOption.Size:
                    return orderedItems.ThenBy(l => l.bundle.entry.size, ascending);
                case BundleSortOption.Time:
                    return orderedItems.ThenBy(l => l.bundle.loadingTime, ascending);
                case BundleSortOption.Path:
                    return orderedItems.ThenBy(l => l.bundle.entry.name, ascending);
                default:
                    return orderedItems.ThenBy(l => l.bundle.entry.name, ascending);
            }
        }

        /// <summary>
        /// build root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem{
                id = 0, depth = -1, displayName = "root"
            };

            List<TreeViewItem> bundleItems = new List<TreeViewItem>();

            // get all bundles
            var bundles = XRes.GetAllAssetBundles();
            if (bundles.Count != 0)
            {
                foreach (var bundle in bundles)
                {
                    bundleItems.Add(new BundleListTreeItem(bundle));
                }
            }

            root.children = bundleItems;

            // setup depths
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        /// <summary>
        /// row GUI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as BundleListTreeItem, args.GetColumn(i), ref args);
        }

        /// <summary>
        /// cell GUI
        /// </summary>
        private bool CellGUI(Rect cellRect, BundleListTreeItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            BundleDisplayState displayState = GetDisplayState(item.bundle);
            switch (displayState)
            {
                case BundleDisplayState.Loading:
                    GUI.color = Color.yellow;
                    break;
                case BundleDisplayState.Unloaded:
                    GUI.color = Color.grey;
                    break;
                default:
                    GUI.color = Color.white;
                    break;
            }
       
            CenterRectUsingSingleLineHeight(ref cellRect);
            BundleColumns myColumns = (BundleColumns)column;

            switch (myColumns)
            {
                case BundleColumns.Bundle:
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

                case BundleColumns.LocationType:
                    if (item.bundle != null)
                    {
                        DefaultGUI.Label(cellRect, item.bundle.entry.locationType.ToString(), args.selected, args.focused);
                    }
                    break;

                case BundleColumns.State:
                    DefaultGUI.Label(cellRect, displayState.ToString(), args.selected, args.focused);
                    break;

                case BundleColumns.RefCount:
                    if (item.bundle != null)
                    {
                        DefaultGUI.Label(cellRect, 
                            $"{item.bundle.refCount}", args.selected, args.focused);
                    }
                    break;

                case BundleColumns.RefMax:
                    if (item.bundle != null)
                    {
                        DefaultGUI.Label(cellRect,
                            $"{item.bundle.maxRefCount}", args.selected, args.focused);
                    }
                    break;

                case BundleColumns.Size:
                    if (item.bundle != null)
                    {
                        DefaultGUI.Label(cellRect,
                            EditorUtility.FormatBytes(item.bundle.entry.size), args.selected, args.focused);
                    }
                    break;

                case BundleColumns.Time:
                    if (item.bundle != null)
                        DefaultGUI.Label(cellRect, item.bundle.loadingTime.ToString("f3"), args.selected, args.focused);
                    break;

                case BundleColumns.Path:
                    if (item.bundle != null)
                        DefaultGUI.Label(cellRect, item.bundle.entry.name, args.selected, args.focused);
                    break;
            }

            GUI.color = oldColor;

            return true;
        }

        /// <summary>
        /// Resolves the current Editor display state for a Bundle.
        /// </summary>
        /// <param name="bundle">The Bundle to inspect.</param>
        /// <returns>The state shown in the Bundle list.</returns>
        private static BundleDisplayState GetDisplayState(XBundle bundle)
        {
            if (bundle == null)
                return BundleDisplayState.Unloaded;

            return BundleDisplayStateResolver.Resolve(
                bundle.isLoading,
                bundle.isDone,
                bundle.bundle != null,
                bundle.refCount,
                bundle.autoReleaseTime);
        }

        /// <summary>
        /// selection changed
        /// </summary>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count > 0)
            {
                var item = FindItem(selectedIds[0], rootItem) as BundleListTreeItem;
                if (item != null)
                {
                    OnBundleSelected?.Invoke(item);
                }
            }
        }

        /// <summary>
        /// create default multi column header state
        /// </summary>
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        /// <summary>
        /// get columns
        /// </summary>
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };

            retVal[0].headerContent = new GUIContent("Bundle", "");
            retVal[0].minWidth = 50;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Type", "");
            retVal[1].minWidth = 30;
            retVal[1].width = 75;
            retVal[1].maxWidth = 100;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = false;

            retVal[2].headerContent = new GUIContent("State", "Current Bundle memory and request state");
            retVal[2].minWidth = 55;
            retVal[2].width = 70;
            retVal[2].maxWidth = 90;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = false;

            retVal[3].headerContent = new GUIContent("Ref", "Active request count");
            retVal[3].minWidth = 30;
            retVal[3].width = 36;
            retVal[3].maxWidth = 60;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = true;

            retVal[4].headerContent = new GUIContent("Max", "Maximum active request count");
            retVal[4].minWidth = 30;
            retVal[4].width = 36;
            retVal[4].maxWidth = 60;
            retVal[4].headerTextAlignment = TextAlignment.Left;
            retVal[4].canSort = true;
            retVal[4].autoResize = true;

            retVal[5].headerContent = new GUIContent("Size", "Size on disk");
            retVal[5].minWidth = 30;
            retVal[5].width = 50;
            retVal[5].maxWidth = 80;
            retVal[5].headerTextAlignment = TextAlignment.Left;
            retVal[5].canSort = true;
            retVal[5].autoResize = true;

            retVal[6].headerContent = new GUIContent("Time(s)", "Bundle loading time in seconds");
            retVal[6].minWidth = 30;
            retVal[6].width = 80;
            retVal[6].maxWidth = 100;
            retVal[6].headerTextAlignment = TextAlignment.Left;
            retVal[6].canSort = true;
            retVal[6].autoResize = false;

            retVal[7].headerContent = new GUIContent("Path", "Bundle path");
            retVal[7].minWidth = 50;
            retVal[7].width = 100;
            retVal[7].maxWidth = 300;
            retVal[7].headerTextAlignment = TextAlignment.Left;
            retVal[7].canSort = true;
            retVal[7].autoResize = true;

            return retVal;
        }

        /// <summary>
        /// columns enum
        /// </summary>
        public enum BundleColumns
        {
            Bundle,
            LocationType,
            State,
            RefCount,
            RefMax,
            Size,
            Time,
            Path
        }
    }

    /// <summary>
    /// Bundle Asset Load Tree Item
    /// </summary>
    public class BundleAssetLoadTreeItem : TreeViewItem
    {
        /// <summary>
        /// bundle info
        /// </summary>
        public AssetLoadInfo assetLoadInfo
        { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="bundleNode"></param>
        public BundleAssetLoadTreeItem(AssetLoadInfo loadInfo)
            : base(loadInfo.path.GetHashCode(), 0, Path.GetFileNameWithoutExtension(loadInfo.path))
        {
            assetLoadInfo = loadInfo;
            icon = AssetDatabase.GetCachedIcon(loadInfo.path) as Texture2D;
        }
    }

    /// <summary>
    /// bundle asset selected handler
    /// </summary>
    /// <param name="assetItem"></param>
    public delegate void BundleAssetSelectedHandler(BundleAssetLoadTreeItem assetItem);

    /// <summary>
    /// Bundle Asset Tree View
    /// </summary>
    public class BundleAssetTreeView : TreeView
    {
        public event BundleAssetSelectedHandler assetSelected;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="mchState"></param>
        public BundleAssetTreeView(TreeViewState state, MultiColumnHeaderState mchState)
            : base(state, new MultiColumnHeader(mchState))
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;

            Reload();
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
                var item = FindItem(selectedIds[0], rootItem) as BundleAssetLoadTreeItem;
                if (item != null)
                {
                    Object target = AssetDatabase.LoadAssetAtPath<Object>(item.assetLoadInfo.path);
                    if (target)
                    {
                        Selection.activeObject = target;
                    }

                    assetSelected?.Invoke(item);
                }
            }
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
        /// sort if needed
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

            SortByColumn();

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
        private bool SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return false;

            List<BundleAssetLoadTreeItem> assetList = new List<BundleAssetLoadTreeItem>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as BundleAssetLoadTreeItem);
            }

            var orderedItems = InitialOrder(assetList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
            return true;
        }

        /// <summary>
        /// initial order
        /// </summary>
        /// <param name="items"></param>
        /// <param name="columnList"></param>
        /// <returns></returns>
        private IOrderedEnumerable<BundleAssetLoadTreeItem> InitialOrder(IEnumerable<BundleAssetLoadTreeItem> items, int[] columnList)
        {
            AnalysisAssetColumns sortOption = (AnalysisAssetColumns)columnList[0];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);

            switch (sortOption)
            {
                case AnalysisAssetColumns.Asset:
                    return items.Order(l => l.displayName, ascending);
                case AnalysisAssetColumns.UseTotal:
                    return items.Order(l => l.assetLoadInfo.usedCount, ascending);
                case AnalysisAssetColumns.ReadTime:
                    return items.Order(l => l.assetLoadInfo.lastReadTime, ascending);
                case AnalysisAssetColumns.LoadTime:
                    return items.Order(l => l.assetLoadInfo.loadingTime, ascending);
                case AnalysisAssetColumns.Path:
                    return items.Order(l => l.assetLoadInfo.path, ascending);
                default:
                    return items.Order(l => l.displayName, ascending);
            }
        }

        /// <summary>
        /// build root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem
            {
                id = 0,
                depth = -1,
                displayName = "root"
            };

            List<TreeViewItem> bundleItems = new List<TreeViewItem>();

            // get all bundles
            var assetLoadInfos = BundleAnalysis.GetAllLoadInfos();
            foreach (var assetLoad in assetLoadInfos) {
                bundleItems.Add(new BundleAssetLoadTreeItem(assetLoad.Value));
            }
            
            root.children = bundleItems;

            // setup depths
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        /// <summary>
        /// row GUI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect cellRect = args.GetCellRect(i);
                CellGUI(cellRect, args.item as BundleAssetLoadTreeItem, args.GetColumn(i), ref args);
            }
        }

        /// <summary>
        /// cell GUI
        /// </summary>
        private void CellGUI(Rect cellRect, BundleAssetLoadTreeItem item, int column, ref RowGUIArgs args)
        {
            if (item == null || item.assetLoadInfo == null)
                return;

            CenterRectUsingSingleLineHeight(ref cellRect);
            AnalysisAssetColumns myColumns = (AnalysisAssetColumns)column;

            switch (myColumns)
            {
                case AnalysisAssetColumns.Asset:
                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.icon != null)
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);

                    DefaultGUI.Label(
                        new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height),
                        item.displayName,
                        args.selected,
                        args.focused);
                    break;

                case AnalysisAssetColumns.UseTotal:
                    DefaultGUI.Label(cellRect, item.assetLoadInfo.usedCount.ToString(), args.selected, args.focused);
                    break;

                case AnalysisAssetColumns.ReadTime:
                    DefaultGUI.Label(cellRect, item.assetLoadInfo.lastReadTime.ToString("F3"), args.selected, args.focused);
                    break;

                case AnalysisAssetColumns.LoadTime:
                    DefaultGUI.Label(cellRect, item.assetLoadInfo.loadingTime.ToString("F3"), args.selected, args.focused);
                    break;

                case AnalysisAssetColumns.Size:
                    DefaultGUI.Label(cellRect, EditorUtility.FormatBytes(item.assetLoadInfo.size), args.selected, args.focused);
                    break;

                case AnalysisAssetColumns.Path:
                    DefaultGUI.Label(cellRect, item.assetLoadInfo.path, args.selected, args.focused);
                    break;
            }
        }

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
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };

            retVal[0].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = false;

            retVal[1].headerContent = new GUIContent("UseTotal", "use total");
            retVal[1].minWidth = 50;
            retVal[1].width = 50;
            retVal[1].maxWidth = 100;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("ReadTime", "Last read time");
            retVal[2].minWidth = 50;
            retVal[2].width = 80;
            retVal[2].maxWidth = 100;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("LoadTime", "load time");
            retVal[3].minWidth = 50;
            retVal[3].width = 80;
            retVal[3].maxWidth = 100;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = true;

            retVal[4].headerContent = new GUIContent("Size", "Size On disk");
            retVal[4].minWidth = 50;
            retVal[4].width = 50;
            retVal[4].maxWidth = 80;
            retVal[4].headerTextAlignment = TextAlignment.Left;
            retVal[4].canSort = true;
            retVal[4].autoResize = true;

            retVal[5].headerContent = new GUIContent("Path", "Asset path");
            retVal[5].minWidth = 50;
            retVal[5].width = 100;
            retVal[5].maxWidth = 300;
            retVal[5].headerTextAlignment = TextAlignment.Left;
            retVal[5].canSort = true;
            retVal[5].autoResize = true;

            return retVal;
        }

        public enum AnalysisAssetColumns
        {            
            Asset,
            UseTotal,
            ReadTime,
            LoadTime,
            Size,
            Path
        }
    }


    /// <summary>
    /// Bundle Analysis Editor
    /// </summary>
    public class BundleAnalysisEditor : IBundleEditor
    {
        /// <summary>
        /// parent window
        /// </summary>
        private BundleEditor window;

        // vertical three-split layout: two splitter positions
        private float topSplitPos = 250f;  
        private float middleSplitPos = 300f;

        /// <summary>
        /// currently dragging splitter
        /// </summary>
        private bool isDraggingTopSplitter = false;
        private bool isDraggingMiddleSplitter = false;

        // TreeView
        private BundleListTreeView bundleListTreeView;
        private BundleAssetTreeView assetTreeView;
        private MultiColumnHeaderState bundleMCHState;
        private MultiColumnHeaderState assetMCHState;

        private SearchField assetSearchField;

        // scroll positions
        private Vector2 bundleScrollPos;
        private Vector2 assetScrollPos;
        private Vector2 messageScrollPos;

        private const float MIN_SPLIT_HEIGHT = 100f;
        private const float SPLITTER_HEIGHT = 5f;
        private const float TOOLBAR_HEIGHT = 0f;

        private TreeViewItem selected;

        /// <summary>        
        /// connect to parent window
        /// </summary>
        /// <param name="editor"></param>
        public BundleAnalysisEditor(BundleEditor editor)
        {
            window = editor;
        }

        /// <summary>
        /// enable widnow
        /// </summary>
        /// <param name="rect"></param>
        public void OnEnable(Rect rect)
        {
            BundleAnalysis.assetLoadHander += OnAssetLoadInfoHandler;
            BundleAnalysis.bundleLoadHander += OnBundleLoadInfoHandler;

            // initialize tree views
            InitializeTreeViews();
            LoadLayout();
        }

        private void OnBundleLoadInfoHandler(XBundle bundle)
        {
            if (bundleListTreeView != null)
                bundleListTreeView.Reload();
        }

        /// <summary>
        /// update asset load info handler
        /// </summary>
        private void OnAssetLoadInfoHandler(AssetLoadInfo loadInfo)
        {
            if (assetTreeView != null)
                assetTreeView.Reload();
        }

        /// <summary>
        /// disable window
        /// </summary>
        public void OnDisable()
        {
            BundleAnalysis.assetLoadHander -= OnAssetLoadInfoHandler;
            BundleAnalysis.bundleLoadHander -= OnBundleLoadInfoHandler;

            SaveLayout();
        }

        /// <summary>
        /// GUI draw
        /// </summary>
        /// <param name="rect"></param>
        public void OnGUI(Rect rect)
        {
            float totalWidth = rect.width;
            float totalHeight = rect.height - TOOLBAR_HEIGHT;
            float toolbarY = rect.y + TOOLBAR_HEIGHT;

            ValidateSplitPositions(totalHeight);

            Rect topSplitterRect = new Rect(0, toolbarY + topSplitPos, totalWidth, SPLITTER_HEIGHT);
            Rect middleSplitterRect = new Rect(0, toolbarY + middleSplitPos + SPLITTER_HEIGHT,
                                             totalWidth, SPLITTER_HEIGHT);

            Rect topRect = new Rect(0, toolbarY, totalWidth, topSplitPos);
            Rect middleRect = new Rect(0, toolbarY + topSplitPos + SPLITTER_HEIGHT,
                                      totalWidth, middleSplitPos - topSplitPos - SPLITTER_HEIGHT);
            Rect bottomRect = new Rect(0, toolbarY + middleSplitPos + (2 * SPLITTER_HEIGHT),
                                      totalWidth, totalHeight - middleSplitPos - (2 * SPLITTER_HEIGHT));

            DrawTopSection(topRect);
            DrawMiddleSection(middleRect);
            DrawBottomSection(bottomRect);
            DrawSplitters(topSplitterRect, middleSplitterRect);
            HandleSplitterDrag(topSplitterRect, middleSplitterRect, rect, totalHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUpdate()
        {
            bundleListTreeView?.ApplyCurrentSort();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnReload()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeTreeViews()
        {
            var bundleTreeViewState = new TreeViewState();
            var bundleHeaderState = BundleListTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(bundleMCHState, bundleHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(bundleMCHState, bundleHeaderState);
            bundleMCHState = bundleHeaderState;

            bundleListTreeView = new BundleListTreeView(bundleTreeViewState, bundleMCHState);
            bundleListTreeView.OnBundleSelected += OnBundleSelected;

            var assetTreeViewState = new TreeViewState();
            var assetHeaderState = BundleAssetTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(assetMCHState, assetHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(assetMCHState, assetHeaderState);
            assetMCHState = assetHeaderState;

            assetTreeView = new BundleAssetTreeView(assetTreeViewState, assetMCHState);
            assetTreeView.assetSelected += OnAssetSelected;

            assetSearchField = new SearchField();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ValidateSplitPositions(float totalHeight)
        {
            float maxTop = totalHeight - (2 * MIN_SPLIT_HEIGHT) - (2 * SPLITTER_HEIGHT);
            topSplitPos = Mathf.Clamp(topSplitPos, MIN_SPLIT_HEIGHT, maxTop);

            float minMiddle = topSplitPos + SPLITTER_HEIGHT + MIN_SPLIT_HEIGHT;
            float maxMiddle = totalHeight - MIN_SPLIT_HEIGHT - SPLITTER_HEIGHT;
            middleSplitPos = Mathf.Clamp(middleSplitPos, minMiddle, maxMiddle);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnBundleSelected(BundleListTreeItem bundleItem)
        {
            if (assetTreeView != null)
                assetTreeView.SetSelection(new List<int> { });

            selected = bundleItem;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnAssetSelected(BundleAssetLoadTreeItem assetItem)
        {
            if (bundleListTreeView != null)
                bundleListTreeView.SetSelection(new List<int> { });

            selected = assetItem;
        }

        /// <summary>
        ///
        /// </summary>
        private void DrawTopSection(Rect rect)
        {
            GUILayout.BeginArea(rect, "Bundles", EditorStyles.helpBox);
            bundleScrollPos = EditorGUILayout.BeginScrollView(bundleScrollPos);

            Rect treeViewRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (bundleListTreeView != null)
            {
                bundleListTreeView.OnGUI(treeViewRect);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// �����м����� - Asset�б�
        /// </summary>
        private void DrawMiddleSection(Rect rect)
        {
            GUI.Box(rect, "", EditorStyles.helpBox);
            Rect contentRect = new Rect(
                rect.x + 2,
                rect.y + 2, 
                rect.width - 4,
                rect.height - 2
            );

            Rect searchRect = new Rect(
                contentRect.x + 5,
                contentRect.y + 5,
                contentRect.width - 10,
                20f
            );
            DrawAssetSearchBar(searchRect);

            Rect treeViewRect = new Rect(
                contentRect.x,
                searchRect.y + searchRect.height + 5,
                contentRect.width,
                contentRect.height - searchRect.height - 10
            );

            if (treeViewRect.height <= 0)
                return;

            Rect treeViewContentRect = new Rect(
                treeViewRect.x,
                treeViewRect.y,
                treeViewRect.width,
                treeViewRect.height
            );

            GUILayout.BeginArea(treeViewContentRect);
            assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos);

            float treeViewHeight = 0f;
            if (assetTreeView != null)
            {
                treeViewHeight = assetTreeView.totalHeight;
                if (treeViewHeight < treeViewContentRect.height)
                {
                    treeViewHeight = treeViewContentRect.height;
                }
            }

            Rect tvRect = GUILayoutUtility.GetRect(
                treeViewContentRect.width,
                treeViewHeight,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );

            if (assetTreeView != null)
            {
                assetTreeView.OnGUI(tvRect);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 
        /// </summary>
        private void DrawBottomSection(Rect rect)
        {
            Rect contentRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 40);

            GUILayout.BeginArea(contentRect, EditorStyles.helpBox);
            messageScrollPos = EditorGUILayout.BeginScrollView(messageScrollPos);

            if (selected != null)
            {
                if (selected is BundleAssetLoadTreeItem a)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(typeof(AssetBundle).Name);
                    if (a.assetLoadInfo.bundleInfo != null)
                        sb.AppendLine($"  {a.assetLoadInfo.bundleInfo.entry.name}");

                    sb.AppendLine(typeof(AssetInfo).Name);
                    sb.AppendLine($"  {a.assetLoadInfo.path}");

                    EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);

                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUIStyle style = new GUIStyle(GUI.skin.label);
                        style.wordWrap = true;

                        GUILayout.Label("Call Stacks:", EditorStyles.boldLabel);

                        var frames = a.assetLoadInfo.frames;
                        foreach (var frame in frames)
                        {
                            if (EditorGUILayout.LinkButton($"{frame.className}.{frame.methodName} " +
                                                         $"at file {Path.GetFileName(frame.fileName)} line {frame.lineNumber}:{frame.columnNumber}"))
                            {
                                InternalEditorUtility.OpenFileAtLineExternal(frame.fileName, frame.lineNumber, frame.columnNumber);
                            }
                        }
                    }
                }
                else if (selected is BundleListTreeItem b)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(typeof(AssetBundle).Name);
                    sb.AppendLine(b.bundle.entry.name);
                    sb.AppendLine(EditorUtility.FormatBytes(b.bundle.entry.size));

                    sb.AppendLine("Assets:");
                    foreach (var file in b.bundle.entry.files)
                    {
                        sb.AppendLine($"  {file.file}");
                    }

                    string dependsOn = b.bundle.entry.dependencies.Length == 0 ? "No dependencies" : "Depends On:";
                    sb.AppendLine($"{dependsOn}");
                    foreach (var file in b.bundle.entry.dependencies)
                    {
                        sb.AppendLine($"  {file}");
                    }

                    EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 
        /// </summary>
        private void DrawAssetSearchBar(Rect rect)
        {
            if (assetSearchField != null && assetTreeView != null)
            {
                assetTreeView.searchString = assetSearchField.OnGUI(rect, assetTreeView.searchString);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void DrawSplitters(Rect topSplitterRect, Rect middleSplitterRect)
        {
            EditorGUI.DrawRect(topSplitterRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUI.DrawRect(middleSplitterRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleSplitterDrag(Rect topSplitterRect, Rect middleSplitterRect, Rect windowRect, float totalHeight)
        {
            Event e = Event.current;

            EditorGUIUtility.AddCursorRect(topSplitterRect, MouseCursor.ResizeVertical);
            EditorGUIUtility.AddCursorRect(middleSplitterRect, MouseCursor.ResizeVertical);

            if (e.type == EventType.MouseDown)
            {
                if (topSplitterRect.Contains(e.mousePosition))
                {
                    isDraggingTopSplitter = true;
                    e.Use();
                }
                else if (middleSplitterRect.Contains(e.mousePosition))
                {
                    isDraggingMiddleSplitter = true;
                    e.Use();
                }
            }


            if (e.type == EventType.MouseDrag)
            {
                float localY = e.mousePosition.y - windowRect.y - TOOLBAR_HEIGHT;

                if (isDraggingTopSplitter)
                {
                    float newTopPos = Mathf.Clamp(localY, MIN_SPLIT_HEIGHT, totalHeight - (2 * MIN_SPLIT_HEIGHT) - (2 * SPLITTER_HEIGHT));
                    if (middleSplitPos < newTopPos + MIN_SPLIT_HEIGHT + SPLITTER_HEIGHT)
                    {
                        middleSplitPos = newTopPos + MIN_SPLIT_HEIGHT + SPLITTER_HEIGHT;
                    }

                    topSplitPos = newTopPos;
                    window.Repaint();
                }
                else if (isDraggingMiddleSplitter)
                {
                    float minY = topSplitPos + SPLITTER_HEIGHT + MIN_SPLIT_HEIGHT;
                    float maxY = totalHeight - MIN_SPLIT_HEIGHT - SPLITTER_HEIGHT;
                    middleSplitPos = Mathf.Clamp(localY, minY, maxY);
                    window.Repaint();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                if (isDraggingTopSplitter || isDraggingMiddleSplitter)
                {
                    isDraggingTopSplitter = false;
                    isDraggingMiddleSplitter = false;
                    SaveLayout();
                    e.Use();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadLayout()
        {
            topSplitPos = EditorPrefs.GetFloat("BundleAnalysis_TopSplit", 250f);
            middleSplitPos = EditorPrefs.GetFloat("BundleAnalysis_MiddleSplit", 300f);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveLayout()
        {
            EditorPrefs.SetFloat("BundleAnalysis_TopSplit", topSplitPos);
            EditorPrefs.SetFloat("BundleAnalysis_MiddleSplit", middleSplitPos);
        }
    }
}
