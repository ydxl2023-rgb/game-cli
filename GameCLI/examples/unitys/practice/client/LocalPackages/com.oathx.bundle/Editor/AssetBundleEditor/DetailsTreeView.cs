using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// Detail Tree View
    /// </summary>
    public class DetailsTreeView : TreeView
    {
        /// <summary>
        /// item id
        /// </summary>
        public int itemId
        { get; private set; } = 0;

        /// <summary>
        /// select bundle
        /// </summary>
        public BundleNode bundle
        { get; private set; }

        /// <summary>
        /// show reference bundle details
        /// </summary>
        public bool showRefBundleDetails
        { get; private set; } = false;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="state"></param>
        public DetailsTreeView(TreeViewState state)
            : base(state)
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            Reload();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bundleNode"></param>
        public void Refresh(BundleNode bundleNode)
        {
            bundle = bundleNode;

            if (bundle)
                Reload();

            ExpandAll();
        }

        /// <summary>
        /// Make Id
        /// </summary>
        /// <returns></returns>
        private int MakeId()
        {
            var idStr = System.Guid.NewGuid().ToString();
            return Animator.StringToHash(idStr);
        }

        /// <summary>
        /// Build Root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            if (bundle)
            {
                // create tree items
                TreeViewItem newDetailTreeItem = new TreeViewItem(MakeId(), 0, bundle.bundlePath);
                newDetailTreeItem.AddChild(new TreeViewItem(MakeId(), 0,
                    "Size On disk: " + bundle.GetFileSize()));

                // create assets item
                TreeViewItem assetsItem = new TreeViewItem(MakeId(), 2, "Assets");
                newDetailTreeItem.AddChild(assetsItem);

                // 预建"资源路径 → 归属 bundle 名"全局映射（基于 Unity 真实归属）
                var assetToBundle = BuildAssetToBundleMap();

                List<string> refBundleNames = new List<string>();
                int depth = 0;
                foreach (var assetInfo in bundle.dependencies)
                {
                    // 从预建映射查真实归属 bundle 名（与处理顺序无关）
                    string ownerBundleName = string.Empty;
                    if (assetToBundle.TryGetValue(assetInfo.path, out string owner))
                    {
                        ownerBundleName = owner;
                    }

                    // 只显示直接资源（属于某个 bundle 的资源），无归属的资源不进 Assets/依赖列表
                    if (!string.IsNullOrEmpty(ownerBundleName))
                    {
                        if (!string.Equals(ownerBundleName, bundle.bundlePath, System.StringComparison.OrdinalIgnoreCase))
                        {
                            // 归属其他 bundle → 依赖的 bundle（按名字去重）
                            if (!refBundleNames.Contains(ownerBundleName))
                            {
                                refBundleNames.Add(ownerBundleName);
                            }
                        }
                        else
                        {
                            // 归属当前 bundle → 直接资源
                            var dependAssetItem = new TreeViewItem(MakeId(), depth++, assetInfo.path);
                            assetsItem.AddChild(dependAssetItem);
                        }
                    }
                }

                TreeViewItem dependBundles = new TreeViewItem(MakeId(), 0, "Dependency bundles");
                newDetailTreeItem.AddChild(dependBundles);

                if (refBundleNames.Count == 0)
                {
                    dependBundles.AddChild(new TreeViewItem(MakeId(), 0, "None"));
                }
                else
                {
                    // proc ref bundles
                    for (int index = 0; index < refBundleNames.Count; index++)
                    {
                        string refBundleName = refBundleNames[index];
                        var bundleItem = new TreeViewItem(MakeId(), index, refBundleName);
                        if (showRefBundleDetails)
                        {
                            // 展开该依赖 bundle 的直接资源（查 Unity 真实归属，与处理顺序无关）
                            int dependDepth = 0;
                            foreach (var refAssetPath in AssetDatabase.GetAssetPathsFromAssetBundle(refBundleName))
                            {
                                var dependAssetItem = new TreeViewItem(MakeId(), dependDepth, Path.GetFileName(refAssetPath));
                                bundleItem.AddChild(dependAssetItem);

                                dependDepth++;
                            }
                        }

                        dependBundles.AddChild(bundleItem);
                    }
                }

                if (bundle.messages.Count > 0)
                {
                    TreeViewItem message = new TreeViewItem(MakeId(), 0, "Messages");
                    for (int idx = 0; idx < bundle.messages.Count; idx++)
                    {
                        var msg = bundle.messages[idx];
                        var msgItem = new TreeViewItem(MakeId(), idx, msg.message);
                        message.AddChild(msgItem);
                    }
                    newDetailTreeItem.AddChild(message);
                }

                root.AddChild(newDetailTreeItem);
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
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
    }
}
