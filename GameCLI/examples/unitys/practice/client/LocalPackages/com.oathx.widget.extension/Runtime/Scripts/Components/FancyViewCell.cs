using UnityEngine;
using UnityEngine.UI;
using FancyScrollView;

namespace Oathx.Widget
{
    public class FancyViewCell : FancyScrollRectCell<VM, Context>
    {
        [SerializeField] 
        protected Button button;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        ViewWaveType waveType = ViewWaveType.None;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        ViewMathType mathType = ViewMathType.None;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected float range;

        /// <summary>
        /// 
        /// </summary>
        protected IWidget widget;

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            if (button == null)
            {
                button = GetComponentInChildren<Button>();
            }

            if (button)
            {
                button.onClick.AddListener(() => {
                    
                    Context.clicked?.Invoke(Index);
                });
            }

            widget = gameObject.GetComponent<IWidget>();
            if (!widget)
                throw new System.ArgumentNullException($"FancyScrollCell Missing [IWidget] component");
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
        /// <param name="normalizedPosition"></param>
        /// <param name="localPosition"></param>
        protected override void UpdatePosition(float normalizedPosition, float localPosition)
        {
            base.UpdatePosition(normalizedPosition, localPosition);

            switch (waveType)
            {
                case ViewWaveType.Left:
                    if (mathType == ViewMathType.Sin)
                    {
                        var leftWave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * range;
                        transform.localPosition += Vector3.left * leftWave;
                    }
                    else if (mathType == ViewMathType.Cos)
                    {
                        var leftWave = Mathf.Cos(normalizedPosition * Mathf.PI * 2) * range;
                        transform.localPosition += Vector3.left * leftWave;
                    }

                    break;

                case ViewWaveType.Right:
                    if (mathType == ViewMathType.Sin)
                    {
                        var rightWave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * range;
                        transform.localPosition += Vector3.right * rightWave;
                    }
                    else if (mathType == ViewMathType.Cos)
                    {
                        var rightWave = Mathf.Cos(normalizedPosition * Mathf.PI * 2) * range;
                        transform.localPosition += Vector3.right * rightWave;
                    }
              
                    break;
            }
        }
    }
}
