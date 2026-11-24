#if UNITY_5_3_OR_NEWER
using UnityEngine;

namespace Oathx.UnityCLI
{
    /// <summary>Constrains the authored rectangle to the device safe area without adding hierarchy nodes.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UnityCliSafeArea : MonoBehaviour
    {
        private RectTransform rect;
        private Vector2 authoredMin;
        private Vector2 authoredMax;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            authoredMin = rect.offsetMin;
            authoredMax = rect.offsetMax;
        }

        private void LateUpdate()
        {
            ApplySafeArea(Screen.safeArea);
        }

        /// <summary>Applies screen-pixel bounds; also allows deterministic simulated device checks.</summary>
        public void ApplySafeArea(Rect safe)
        {
            if (safe.width <= 0 || safe.height <= 0)
            {
                return;
            }
            RectTransform parent = rect.parent as RectTransform;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (parent == null || canvas == null || canvas.renderMode == RenderMode.WorldSpace)
            {
                return;
            }
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safe.min, camera, out Vector2 safeMin)
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safe.max, camera, out Vector2 safeMax))
            {
                return;
            }
            // Always start from authored offsets, avoiding cumulative movement on rotation/resizing.
            Vector2 anchorMin = parent.rect.min + Vector2.Scale(parent.rect.size, rect.anchorMin);
            Vector2 anchorMax = parent.rect.min + Vector2.Scale(parent.rect.size, rect.anchorMax);
            Vector2 min = anchorMin + authoredMin;
            Vector2 max = anchorMax + authoredMax;
            for (int axis = 0; axis < 2; axis++)
            {
                float size = Mathf.Min(max[axis] - min[axis], safeMax[axis] - safeMin[axis]);
                min[axis] = Mathf.Clamp(min[axis], safeMin[axis], safeMax[axis] - size);
                max[axis] = min[axis] + size;
            }
            rect.offsetMin = min - anchorMin;
            rect.offsetMax = max - anchorMax;
        }
    }
}
#endif
