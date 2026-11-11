using System;
using System.Collections.Generic;
using FancyScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace Oathx.Widget
{
    /// <summary>
    /// 
    /// </summary>
    public class FancyAnimatorViewCell : FancyCell<VM, Context>
    {
        protected float currentPosition = 0;

        [SerializeField]
        protected Button button;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected Animator animator;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected string scrollClipName;

        /// <summary>
        /// 
        /// </summary>
        protected IWidget widget;

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            UpdatePosition(currentPosition);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (button)
            {
                button.onClick.AddListener(() => {
                    Context.clicked?.Invoke(Index);
                });
            }

            widget = gameObject.GetComponent<IWidget>();
            if (!widget)
                throw new System.NullReferenceException("FancyAnimatorViewCell missing IWidget component");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        public override void UpdateContent(VM vm)
        {
            if (widget)
            {
                var old = widget.GetVM();
                if (old)
                    widget.Unref(old);

                widget.Ref(vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            if (animator)
            {
                if (animator.isActiveAndEnabled)
                {
                    animator.Play(scrollClipName, -1, position);
                }

                animator.speed = 0;
            }
        }
    }
}
