using System;
using UnityEngine;
using EasingCore;
using FancyScrollView;
using System.Collections.Generic;

namespace Oathx.Widget
{
    /// <summary>
    /// 
    /// </summary>
    public class Context : FancyGridViewContext
    {
        public int selectedIndex = -1;

        /// <summary>
        /// 
        /// </summary>
        public Action<int> clicked;
    }

    public enum Alignment
    {
        Upper,
        Middle,
        Lower,
    }

    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Scroller))]
    public class FancyGridScrollView : FancyGridView<VM, Context>
    {
        /// <summary>
        /// 
        /// </summary>
        class CellGroup : DefaultCellGroup
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        FancyGridScrollViewCell cellPrefab;

        /// <summary>
        /// 
        /// </summary>
        protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);

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
            if (cellPrefab)
                return cellPrefab.gameObject;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected IList<VM> items = new List<VM>();

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
        public float SpacingY
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
        public float SpacingX
        {
            get => startAxisSpacing;
            set
            {
                startAxisSpacing = value;
                Relayout();
            }
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
        /// <param name="index"></param>
        /// <returns></returns>
        public bool UpdateSelection(int index)
        {
            if (Context.selectedIndex == index)
                return false;

            Context.selectedIndex = index;
            Refresh();

            return true;
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
        protected float GetAlignment(Alignment alignment)
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
    }
}
