using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Oathx.Entity.Editor
{
    /// <summary>
    /// xentity attr element
    /// </summary>
    public class XEntityAttrElement : TreeViewItem
    {
        /// <summary>
        /// The entity.
        /// </summary>
        public XEntity entity
        { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="depth"></param>
        /// <param name="displayName"></param>
        /// <param name="actorEntity"></param>
        public XEntityAttrElement(int id, int depth, string displayName, XEntity actorEntity)
            : base(id, depth, displayName)
        {
            entity = actorEntity;
            children = new List<TreeViewItem>();
        }
    }

    /// <summary>
    /// xentity attr tree view
    /// </summary>
    public class XEntityAttrTreeView : TreeView
    {
        /// <summary>
        /// The entity.
        /// </summary>
        private XEntity entity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="mchs"></param>
        public XEntityAttrTreeView(TreeViewState state, MultiColumnHeaderState mchs)
            : base(state, new MultiColumnHeader(mchs))
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        /// <summary>
        /// The my columns.
        /// </summary>
        public enum MyColumns
        {
            ID,
            Value,
            Base,
            Add,
            Mul
        }

        /// <summary>
        /// The sort options.
        /// </summary>
        private MyColumns[] sortOptions = {
            MyColumns.ID,
            MyColumns.Value,
            MyColumns.Base,
            MyColumns.Mul
        };

        /// <summary>
        /// row GUI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as XEntityAttrElement, args.GetColumn(i), ref args);
        }

        /// <summary>
        /// cell GUI
        /// </summary>
        /// <param name="cellRect"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        /// <param name="args"></param>
        private void CellGUI(Rect cellRect, XEntityAttrElement item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);


            MyColumns myColumns = (MyColumns)column;
            switch (myColumns)
            {
                case MyColumns.ID:
                    {
                        DefaultGUI.Label(
                            cellRect,
                            item.displayName,
                            args.selected,
                            args.focused);
                    }
                    break;
                case MyColumns.Value:
                    if (item.entity)
                        DefaultGUI.Label(cellRect, item.entity.GetAttr(item.id).ToString(), args.selected, args.focused);
                    break;
                case MyColumns.Base:
                    if (item.entity)
                        DefaultGUI.Label(cellRect, item.entity.GetBaseValue(item.id).ToString(), args.selected, args.focused);
                    break;
                case MyColumns.Add:
                    if (item.entity)
                        DefaultGUI.Label(cellRect, item.entity.GetAddValue(item.id).ToString(), args.selected, args.focused);
                    break;
                case MyColumns.Mul:
                    if (item.entity)
                        DefaultGUI.Label(cellRect, item.entity.GetMulValue(item.id).ToString(), args.selected, args.focused);
                    break;
            }
            GUI.color = oldColor;
        }

        /// <summary>
        /// build root
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            var root = new XEntityAttrElement(0, -1, "", new XEntity());
            if (entity != null)
            {
                var numbers = entity.GetAttrs();
                if (numbers != null)
                {
                    for (int index = 0; index < numbers.Length; index++)
                    {
                        XEntityAttrElement element = new XEntityAttrElement(index, root.depth + 1, index.ToString(), entity);
                        root.AddChild(element);
                    }
                }
            }

            return root;
        }

        /// <summary>
        /// set select entitys
        /// </summary>
        /// <param name="entity"></param>
        public void SetSelectEntitys(XEntity selectEntity)
        {
            entity = selectEntity;
            if (entity != null)
                Reload();
        }

        /// <summary>
        /// get select entity
        /// </summary>
        /// <returns></returns>
        public XEntity GetSelectEntity()
        {
            return entity;
        }

        /// <summary>
        /// if need sort
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
        /// sorting changed
        /// </summary>
        /// <param name="multiColumnHeader"></param>
        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
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

            List<XEntityAttrElement> assetList = new List<XEntityAttrElement>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as XEntityAttrElement);
            }
            var orderedItems = InitialOrder(assetList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();

            return true;
        }

        /// <summary>
        /// order initial
        /// </summary>
        /// <param name="myTypes"></param>
        /// <param name="columnList"></param>
        /// <returns></returns>
        private IOrderedEnumerable<XEntityAttrElement> InitialOrder(IEnumerable<XEntityAttrElement> myTypes, int[] columnList)
        {
            MyColumns sortOption = sortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case MyColumns.ID:
                    return myTypes.Order(l => l.id, ascending);
                default:
                    return myTypes.Order(l => l.displayName, ascending);
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
                new MultiColumnHeaderState.Column()
            };

            retVal[0].headerContent = new GUIContent("ID");
            retVal[0].minWidth = 30;
            retVal[0].width = 70;
            retVal[0].maxWidth = 100;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Value");
            retVal[1].minWidth = 50;
            retVal[1].width = 100;
            retVal[1].maxWidth = 150;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Base");
            retVal[2].minWidth = 50;
            retVal[2].width = 100;
            retVal[2].maxWidth = 150;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("Add");
            retVal[3].minWidth = 30;
            retVal[3].width = 75;
            retVal[3].maxWidth = 150;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = true;

            retVal[4].headerContent = new GUIContent("Mul");
            retVal[4].minWidth = 30;
            retVal[4].width = 75;
            retVal[4].maxWidth = 150;
            retVal[4].headerTextAlignment = TextAlignment.Left;
            retVal[4].canSort = true;
            retVal[4].autoResize = false;

            return retVal;
        }
    }

    /// <summary>
    /// entity extension methods
    /// </summary>
    static class EntityExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}
