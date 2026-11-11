using UnityEngine;
using UnityEngine.UI;
using FancyScrollView;

namespace Oathx.Widget
{
    public enum ViewWaveType
    {
        None,
        Left,
        Right
    }

    public enum ViewMathType
    {
        None,
        Sin,
        Cos
    }

    public class FancyGridScrollViewCell : FancyGridViewCell<VM, Context>
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        Button button;

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
        float wave = 0;

        /// <summary>
        /// 
        /// </summary>
        protected IWidget widget;

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(() => {
                    Context.clicked?.Invoke(Index);
                });
            }
                
            widget = gameObject.GetComponent<IWidget>();
            if (!widget)
                throw new System.NullReferenceException("FancyGridViewCell missing IWidget component");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemData"></param>
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

            switch(waveType)
            {
                case ViewWaveType.Left:
                    var leftWave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * wave;
                    transform.localPosition += Vector3.left * leftWave;
                    break;

                case ViewWaveType.Right:
                    var rightWave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * wave;
                    transform.localPosition += Vector3.right * rightWave;
                    break;
            }
        }
    }
}
