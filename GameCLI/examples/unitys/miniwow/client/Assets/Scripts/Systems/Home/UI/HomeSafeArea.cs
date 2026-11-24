using UnityEngine;

/// <summary>Keeps authored Home controls inside the device safe area.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class HomeSafeArea : MonoBehaviour
{
    /// <summary>The transform owned by this component.</summary>
    private RectTransform target;

    /// <summary>Last applied physical safe rectangle.</summary>
    private Rect previousArea;

    /// <summary>Last observed physical screen size.</summary>
    private Vector2Int previousSize;

    /// <summary>Initializes the component's own transform and applies current insets.</summary>
    private void OnEnable()
    {
        target = GetComponent<RectTransform>();
        Apply();
    }

    /// <summary>Updates insets after a resolution or orientation change.</summary>
    private void LateUpdate()
    {
        if (previousArea != Screen.safeArea || previousSize.x != Screen.width || previousSize.y != Screen.height)
            Apply();
    }

    /// <summary>Converts physical screen insets into normalized canvas anchors.</summary>
    private void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        previousArea = Screen.safeArea;
        previousSize = new Vector2Int(Screen.width, Screen.height);
        target.anchorMin = new Vector2(previousArea.xMin / Screen.width, previousArea.yMin / Screen.height);
        target.anchorMax = new Vector2(previousArea.xMax / Screen.width, previousArea.yMax / Screen.height);
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }
}
