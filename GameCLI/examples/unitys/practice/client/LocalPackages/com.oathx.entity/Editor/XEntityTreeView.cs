using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Oathx.Entity.Editor
{
    [Serializable]
    public class XEntityElement : TreeViewItem
    {
        /// <summary>
        /// entity info
        /// </summary>
        public XEntity entity
        { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="depth"></param>
        public XEntityElement(XEntity entityInfo, int depth)
            : base(entityInfo.iid, depth, entityInfo.GetType().Name)
        {
            children = new List<TreeViewItem>();
            entity = entityInfo;
        }
    }

    /// <summary>
    /// entity tree view
    /// </summary>
    public class XEntityTreeView : TreeView
    {
        /// <summary>
        /// entity editor window
        /// </summary>
        private XEntityEditorWindow entityEditorWindow;        

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="window"></param>
        public XEntityTreeView(TreeViewState state, XEntityEditorWindow window)
            : base(state)
        {
            entityEditorWindow = window;

            showAlternatingRowBackgrounds = true;
            showBorder = true;

            Reload();
        }

        /// <summary>
        /// build root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            return CreateTreeView();
        }

        /// <summary>
        /// create tree view
        /// </summary>
        /// <returns></returns>
        protected TreeViewItem CreateTreeView()
        {
            var result = new XEntityElement(new XEntity(), -1);

            var entitys = XEntityManager.GetSingleton().GetEntitys();
            foreach (var it in entitys)
            {
                var treeNode = new XEntityElement(it.Value, result.depth + 1);
                result.AddChild(treeNode);

                BuildTreeNode(it.Value, treeNode);
            }

            // setup depths
            SetupDepthsFromParentsAndChildren(result);

            var selectIds = GetSelection();
            if (selectIds.Count != 0)
                SelectionChanged(selectIds);

            return result;
        }

        /// <summary>
        /// build tree node
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parent"></param>
        private void BuildTreeNode(XEntity entity, XEntityElement parent)
        {
            //foreach (var child in entity.children)
            //{
            //    var childNode = new XEntityElement(child.Value, parent.depth + 1);
            //    parent.AddChild(childNode);

            //    if (child.Value.children.Count != 0)
            //        BuildTreeNode(child.Value, childNode);
            //}
        }

        /// <summary>
        /// selection changed
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            var selectEntitys = new List<XEntity>();
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var item = FindItem(id, rootItem) as XEntityElement;
                    if (item != null && item.entity != null)
                    {
                        selectEntitys.Add(item.entity);
                        break;
                    }
                }
            }

            if (selectEntitys.Count != 0)
            {
                entityEditorWindow.UpdateEntityAttr(selectEntitys);
            }
        }
    }
}
