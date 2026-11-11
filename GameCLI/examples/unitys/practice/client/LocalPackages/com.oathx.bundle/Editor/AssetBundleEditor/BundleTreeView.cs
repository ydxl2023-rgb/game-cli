using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// bundle tree
    /// </summary>
    public static class BundleTree
    {
        static readonly Dictionary<PackageType, BundlePackage> packages = new Dictionary<PackageType, BundlePackage>();

        /// <summary>
        /// Add Package
        /// </summary>
        /// <param name="type"></param>
        /// <param name="package"></param>
        public static void AddPackage(PackageType type, BundlePackage package)
        {
            if (!packages.ContainsKey(type))
            {
                packages.Add(type, package);
            }
        }

        /// <summary>
        /// Get Packages
        /// </summary>
        /// <returns></returns>
        public static Dictionary<PackageType, BundlePackage> GetPackages()
        {
            return packages;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsEmpty()
        {
            return packages.Count > 0;
        }

        /// <summary>
        /// Remove Package
        /// </summary>
        /// <param name="type"></param>
        public static void RemovePackage(PackageType type)
        {
            if (packages.ContainsKey(type))
            {
                packages.Remove(type);
            }
        }

        /// <summary>
        /// Get Root Node
        /// </summary>
        /// <returns></returns>
        public static BundleNode GetRoot(PackageType packageType=PackageType.Default)
        {
            if (!packages.TryGetValue(packageType, out BundlePackage package))
            {
                package = new BundlePackage(packageType.ToString(), Application.dataPath);
                packages.Add(packageType, package);
            }

            return package;
        }

        /// <summary>
        /// Add Bundle Path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool AddBundle(string path, BundleNode parent=null)
        {
            string[] parts = path.ToLower().Replace("\\", "/").Split('/');
            if (parts.Length <= 0) 
                return false;

            BundleNode currentNode = parent ? parent : GetRoot();
            for (int i=0; i<parts.Length; i++)
            {
                string part = parts[i];
                bool isLastPart = i == parts.Length - 1;

                string currentPath = string.Join("/", parts.Take(i + 1));
                BundleNode existingNode = currentNode.FindChild(part);
                if (!existingNode)
                {
                    string bundleName = part;
                    if (isLastPart)
                        bundleName = Path.ChangeExtension(part, null);

                    currentNode = currentNode.AddChild(bundleName, path.ToLower(), isLastPart, 
                        isLastPart ? BundleNodeType.File : BundleNodeType.Folder);
                }
                else
                {
                    if (isLastPart)
                        existingNode.bundleType = BundleNodeType.File;
                    else
                        existingNode.bundleType = BundleNodeType.Folder;

                    currentNode = existingNode;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove Bundle Path
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool RemoveBundle(string name, BundleNode parent)
        {
            var existingNode = parent.FindChild(name);
            if (existingNode)
            {
                parent.children.Remove(existingNode);
                return true;
            }

            return false;
        }

        /// <summary>
        /// builder tree view items
        /// </summary>
        /// <param name="startId"></param>
        /// <returns></returns>
        public static BundleTreeItem Builder(PackageType packageType=PackageType.Default)
        {
            var rootNode = GetRoot(packageType);
            if (!rootNode)
                throw new NullReferenceException();

            BundleTreeItem rootItem = new BundleTreeItem(Animator.StringToHash(rootNode.bundleName), -1, Path.GetFileName(rootNode.bundleName));
            rootItem.icon = GetIconForNode(rootNode);
            rootItem.bundle = rootNode;

            // build children
            BuildTreeViewItemRecursive(rootNode, rootItem, 0);

            return rootItem;
        }

        /// <summary>
        /// recursive build tree view items
        /// </summary>
        static int BuildTreeViewItemRecursive(BundleNode node, BundleTreeItem parentItem, int currentId)
        {
            foreach (var child in node.children)
            {
                BundleTreeItem treeViewItem = new BundleTreeItem(currentId++, parentItem.depth + 1, child.bundleName);
                treeViewItem.icon = GetIconForNode(child);
                treeViewItem.bundle = child;

                if (parentItem.children == null)
                    parentItem.children = new List<TreeViewItem>();

                parentItem.children.Add(treeViewItem);
                if (child.children.Count > 0)
                {
                    currentId = BuildTreeViewItemRecursive(child, treeViewItem, currentId);
                }
            }

            return currentId;
        }

        /// <summary>
        /// Get Icon For Node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Texture2D GetIconForNode(BundleNode node)
        {
            if (node.bundleType != BundleNodeType.File)
                return EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;

            return null;
        }

        /// <summary>
        /// Get File Icon by Extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Texture2D GetFileIcon(string extension)
        {
            switch (extension)
            {
                case ".cs":
                    return EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
                case ".unity":
                    return EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
                case ".prefab":
                    return EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                    return EditorGUIUtility.IconContent("Texture Icon").image as Texture2D;
                case ".mat":
                    return EditorGUIUtility.IconContent("Material Icon").image as Texture2D;
                case ".fbx":
                case ".obj":
                    return EditorGUIUtility.IconContent("Mesh Icon").image as Texture2D;
                case ".shader":
                    return EditorGUIUtility.IconContent("Shader Icon").image as Texture2D;
                case ".anim":
                    return EditorGUIUtility.IconContent("Animation Icon").image as Texture2D;
                case ".controller":
                    return EditorGUIUtility.IconContent("AnimatorController Icon").image as Texture2D;
                default:
                    return EditorGUIUtility.IconContent("TextAsset Icon").image as Texture2D;
            }
        }

        /// <summary>
        /// clear all bundles
        /// </summary>
        public static void ClearBundles()
        {
            foreach (var package in packages.Values)
            {
                package.children.Clear();
            }
        }

        /// <summary>
        /// refresh bundles
        /// </summary>
        public static bool RefreshBundles()
        {
            // clear bundle all tree node
            ClearBundles();

            // clear all bundle dependencys
            BundleDependencyCollector.ClearDependencys();

            // remove all asset bundle names
            AssetDatabase.RemoveUnusedAssetBundleNames();

            // get current used bundle paths
            var bundles = AssetDatabase.GetAllAssetBundleNames();
            if (bundles.Length != 0)
            {
                foreach (var path in bundles)
                {
                    AddBundle(path);
                }
            }
        
            return true;
        }

        /// <summary>
        /// update bundle dependencys
        /// </summary>
        public static void UpdateBundleDependencys()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            BundleDependencyCollector.UpdateBundleDependencys(new List<BundleNode>() { GetRoot()});
            stopwatch.Stop();
        }

        /// <summary>
        /// set asset bundle name and variant
        /// </summary>
        /// <param name="path"></param>
        public static bool SetAssetBundleNameAndVariant(string path)
        {
            string relativePath = path.ToLower().Replace("\\", "/");
            AssetImporter importer = AssetImporter.GetAtPath(relativePath);
            if (!importer)
                return false;

            AssetDatabase.RemoveUnusedAssetBundleNames();

            // check can set bundle name
            string bundleName = Path.ChangeExtension(relativePath, null);
            if (!CheckBundleCanSet(bundleName))
            { 
                EditorUtility.DisplayDialog("Error",
                    $"Cannot set bundle name '{bundleName}' because a parent folder is already assigned as a bundle.",
                    "OK");
                return false;
            }

            // set bundle name
            AssetDatabase.StartAssetEditing();
            importer.SetAssetBundleNameAndVariant(bundleName, string.Empty);
            AssetDatabase.StopAssetEditing();

            return true;
        }

        /// <summary>
        /// add bundle can set
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckBundleCanSet(string path)
        {
            string[] bundles = AssetDatabase.GetAllAssetBundleNames();
            if (bundles.Length <= 0)
                return true;

            string[] array = path.Split("/");

            // check parent bundle exist
            var parts = string.Empty;
            int index = 0;
            while (index < array.Length)
            {
                parts = index == 0 ? array[index] : Path.Combine(parts, array[index]).Replace("\\", "/");
                if (bundles.Contains(parts))
                    return false;

                index++;
            }

            return true;
        }
    }

    /// <summary>
    /// New Bundle Tree Item
    /// </summary>
    public class BundleTreeItem : TreeViewItem
    {
        /// <summary>
        /// Bundle Name
        /// </summary>
        public string bundleName
        { get; set; }

        /// <summary>
        /// Asset Count
        /// </summary>
        public int assetCount
        { get; set; }

        /// <summary>
        /// Bundle Size
        /// </summary>
        public long bundleSize
        { get; set; }

        /// <summary>
        /// Node Data
        /// </summary>
        public BundleNode bundle
        { get; set; }

        /// <summary>
        /// New Bundle Tree Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="depth"></param>
        /// <param name="displayName"></param>
        public BundleTreeItem(int id, int depth, string displayName)
            : base(id, depth, displayName)
        {
            bundleName = displayName;
            assetCount = 0;
            bundleSize = 0;
        }
    }

    /// <summary>
    /// Bundle Selected Handler
    /// </summary>
    /// <param name="bundleItem"></param>
    public delegate void BundleSelectedHandler(BundleTreeItem bundleItem);

    /// <summary>
    /// New Bundle Tree View
    /// </summary>
    public class BundleTreeView : TreeView
    {
        public event BundleSelectedHandler OnBundleSelected;

        /// <summary>
        /// New Bundle Tree View
        /// </summary>
        /// <param name="state"></param>
        public BundleTreeView(TreeViewState state)
            : base(state)
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            Reload();
        }

        /// <summary>
        /// Build Root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "" };
            if (!BundleTree.IsEmpty())
                BundleTree.AddPackage(PackageType.Default, new BundlePackage(PackageType.Default.ToString(), Application.dataPath));

            List<TreeViewItem> packageItems = new List<TreeViewItem>();
            var packages = BundleTree.GetPackages();
            foreach(var package in packages)
            {
                var packageRoot = BundleTree.Builder(package.Key);
                if (packageRoot != null)
                {
                    packageItems.Add(packageRoot);
                }
            }

            root.children = packageItems;

            BundleTree.UpdateBundleDependencys();

            // setup depths
            SetupDepthsFromParentsAndChildren(root);
 
            return root;
        }

        /// <summary>
        /// Refresh
        /// </summary>
        public void Refresh()
        {
            bool succeed = BundleTree.RefreshBundles();
            if (succeed)
                Reload();
            
            ExpandAll();
        }

        /// <summary>
        /// Build Rows
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        /// <summary>
        /// Row GUI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as BundleTreeItem;

            Rect toggleRect = args.rowRect;
            toggleRect.x += GetContentIndent(item);
            toggleRect.width = 16;

            Rect iconRect = args.rowRect;
            iconRect.x += GetContentIndent(item) + 20;
            iconRect.width = 16;
            if (item.icon != null)
                GUI.DrawTexture(iconRect, item.icon);

            Rect labelRect = args.rowRect;
            labelRect.x += GetContentIndent(item) + 40;
            labelRect.width -= GetContentIndent(item) + 40;

            EditorGUI.LabelField(labelRect, item.displayName);

            var bundle = item.bundle;
            if (bundle)
            {
                Texture icon = null;
                switch (bundle.state)
                {
                    case ErrorState.Warning:
                        icon = EditorGUIUtility.FindTexture("console.warnicon");
                        break;

                    case ErrorState.Error:
                        icon = EditorGUIUtility.FindTexture("console.errorIcon");
                        break;
                }

                if (icon)
                {
                    var size = args.rowRect.height;
                    var right = args.rowRect.xMax;
                    Rect messageRect = new Rect(right - size, args.rowRect.yMin, size, size);
                    GUI.Label(messageRect, new GUIContent(icon));
                }
            }
        }

        /// <summary>
        /// Selection Changed
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count > 0)
            {
                var item = FindItem(selectedIds[0], rootItem) as BundleTreeItem;
                if (item != null)
                {
                    OnBundleSelected?.Invoke(item);
                }
            }
        }

        /// <summary>
        /// Handle Drag And Drop
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    return HandleDragUpdated();

                case EventType.DragPerform:
                    return HandleDragPerform(args);

                case EventType.DragExited:
                    break;
            }

            return DragAndDropVisualMode.None;
        }

        /// <summary>
        /// proc ess drag updated event
        /// </summary>
        private DragAndDropVisualMode HandleDragUpdated()
        {
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
            {
                return DragAndDropVisualMode.None;
            }

            bool hasValidPaths = false;
            foreach (var path in DragAndDrop.paths)
            {
                if (IsValidAssetPath(path))
                {
                    hasValidPaths = true;
                    break;
                }
            }

            if (!hasValidPaths)
            {
                return DragAndDropVisualMode.Rejected;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();

            return DragAndDropVisualMode.Copy;
        }

        /// <summary>
        /// process drag perform event
        /// </summary>
        private DragAndDropVisualMode HandleDragPerform(DragAndDropArgs args)
        {
            var target = args.parentItem as BundleTreeItem;

            List<string> validPaths = new List<string>();
            foreach (var path in DragAndDrop.paths)
            {
                if (IsValidAssetPath(path))
                {
                    validPaths.Add(path);
                }
            }

            if (validPaths.Count == 0)
            {
                return DragAndDropVisualMode.Rejected;
            }

            ProcessDroppedPaths(validPaths, target);

            DragAndDrop.AcceptDrag();
            Event.current.Use();

            Refresh();

            return DragAndDropVisualMode.Copy;
        }

        /// <summary>
        /// check if the path is a valid asset path
        /// </summary>
        private bool IsValidAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (!path.StartsWith("Assets/") && !path.StartsWith(Application.dataPath))
                return false;

            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// proc ess dropped paths
        /// </summary>
        private void ProcessDroppedPaths(List<string> paths, BundleTreeItem parent)
        {
            foreach (var path in paths)
            {
                BundleTree.SetAssetBundleNameAndVariant(path);
            }
        }
        
        /// <summary>
        /// Context Clicked
        /// </summary>
        protected override void ContextClicked()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Pacakge"), false, CreateExtensiblePackage);
            menu.AddItem(new GUIContent("Delete Bundle"), false, DeleteSelectedBundle);
            menu.AddItem(new GUIContent("Build Bundle"), false, BuildSelectedBundle);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Refresh"), false, RefreshBundles);
            menu.ShowAsContext();
        }

        /// <summary>
        /// create package
        /// </summary>
        private void CreateExtensiblePackage() 
        {
            BundleTree.AddPackage(PackageType.Extensible, 
                new BundlePackage(PackageType.Extensible.ToString(), Application.dataPath));

            Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeleteSelectedBundle()
        {
            var selecteds = GetSelection();
            foreach (var selected in selecteds)
            {
                var item = FindItem(selected, rootItem) as BundleTreeItem;
                if (item != null)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(item.bundle.bundlePath);
                    if (importer)
                    {
                        AssetDatabase.StartAssetEditing();
                        importer.SetAssetBundleNameAndVariant(string.Empty, string.Empty);
                        AssetDatabase.StopAssetEditing();
                    }
                    else
                    {
                        if (item.bundle.dependencies.Count <= 0)
                        {
                            var parent = item.bundle.parent;
                            if (parent)
                                parent.RemoveChild(item.bundle);
                        }
                        else
                        {
                            AssetDatabase.RemoveAssetBundleName(item.bundle.bundlePath, true);
                        }
                    }
                }
            }

            Refresh();
        }

        private void BuildSelectedBundle()
        {
        
        }

        private void RefreshBundles()
        {
            Refresh();
        }
    }
}
