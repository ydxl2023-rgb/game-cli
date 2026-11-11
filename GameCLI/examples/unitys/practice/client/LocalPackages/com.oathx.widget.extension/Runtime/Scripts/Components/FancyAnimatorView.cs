using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FancyScrollView;

namespace Oathx.Widget
{
    [RequireComponent(typeof(Scroller))]
    public class FancyAnimatorView : FancyScrollView<VM, Context>
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected Scroller scroller;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected GameObject cellPrefab;

        /// <summary>
        /// 
        /// </summary>
        protected override GameObject CellPrefab => GetCellPrefab();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GameObject GetCellPrefab()
        {
            return cellPrefab;
        }

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
        protected override void Initialize()
        {
            base.Initialize();

            if (scroller)
                scroller.OnValueChanged(UpdatePosition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public virtual void UpdateData(IList<VM> items)
        {
            UpdateContents(items);

            if (scroller)
                scroller.SetTotalCount(items.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void SetCellClicked(System.Action<int> callback)
        {
            Context.clicked = callback;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public virtual void Add(VM element)
        {
            if (element)
            {
                ItemsSource.Add(element);
                
                if (scroller)
                    scroller.SetTotalCount(ItemsSource.Count);

                Refresh();
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
                var index = ItemsSource.IndexOf(element);
                if (index >= 0)
                {
                    ItemsSource.RemoveAt(index);

                    if (scroller)
                        scroller.SetTotalCount(ItemsSource.Count);

                    Refresh();
                }    
            }
        }

        public virtual void Modify(int index, VM element)
        {
            if (element)
            {
                if (index >= 0 && index < ItemsSource.Count)
                {
                    ItemsSource[index] = element;
                    Refresh();
                }
            }
        }
    }
}
