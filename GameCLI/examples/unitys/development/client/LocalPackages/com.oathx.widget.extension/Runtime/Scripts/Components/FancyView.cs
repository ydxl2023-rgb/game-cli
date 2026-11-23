using System;
using System.Collections.Generic;
using UnityEngine;
using EasingCore;
using FancyScrollView;

namespace Oathx.Widget
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Scroller))]
    public class FancyView : FancyScrollRect<VM, Context>
    {
        protected List<VM> items = new List<VM>();

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected float cellSize = 100f;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected GameObject cellPrefab;

        /// <summary>
        /// 
        /// </summary>
        protected override float CellSize => GetCellSize();

        /// <summary>
        /// 
        /// </summary>
        protected override GameObject CellPrefab => GetCellPrefab();

        /// <summary>
        /// 
        /// </summary>
        public int DataCount => GetDataCount();

        /// <summary>
        /// 
        /// </summary>
        public Transform CellContainer
        {
            get => cellContainer;
            set
            {
                cellContainer = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetCellPrefab()
        {
            return cellPrefab;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual float GetCellSize()
        {
            return cellSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual int GetDataCount()
        {
            return ItemsSource.Count;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public float PaddingTop
        {
            get => paddingHead;
            set
            {
                paddingHead = value;
                Relayout();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float PaddingBottom
        {
            get => paddingTail;
            set
            {
                paddingTail = value;
                Relayout();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                Relayout();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void SetCellClicked(Action<int> callback)
        {
            Context.clicked = callback;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectIndex(int index)
        {
            Context.selectedIndex = index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetSelectIndex()
        {
            return Context.selectedIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public virtual void Add(VM element)
        {
            if (element)
            {
                items.Add(element);
                UpdateContents(items);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public virtual void Remove(VM element)
        {
            if (element)
            {
                int index = items.IndexOf(element);
                if (index >= 0)
                {
                    items.RemoveAt(index);
                    UpdateContents(items);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void UpdateData(IList<VM> items)
        {
            UpdateContents(items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="duration"></param>
        /// <param name="easing"></param>
        /// <param name="alignment"></param>
        public void ScrollTo(int index, float duration, Ease easing, Alignment alignment = Alignment.Middle)
        {
            UpdateSelection(index);
            ScrollTo(index, duration, easing, GetAlignment(alignment));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="alignment"></param>
        public void JumpTo(int index, Alignment alignment = Alignment.Middle)
        {
            UpdateSelection(index);
            JumpTo(index, GetAlignment(alignment));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns></returns>
        private float GetAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Upper: 
                    return 0.0f;
                case Alignment.Middle: 
                    return 0.5f;
                case Alignment.Lower: 
                    return 1.0f;
            }

            return GetAlignment(Alignment.Middle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool UpdateSelection(int index)
        {
            if (Context.selectedIndex == index)
                return false;

            Context.selectedIndex = index;
            Refresh();

            return true;
        }
    }
}
