using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Fits the authored notice entry and scroll content to local body text.
/// </summary>
public class NoticeLayout : UIBehaviour
{
    /// <summary>
    /// Body text whose preferred height determines the entry size.
    /// </summary>
    [SerializeField]
    private Text body;

    /// <summary>
    /// Expandable body region.
    /// </summary>
    [SerializeField]
    private RectTransform expanded;

    /// <summary>
    /// Authored notice entry root.
    /// </summary>
    [SerializeField]
    private RectTransform entry;

    /// <summary>
    /// ScrollRect content root.
    /// </summary>
    [SerializeField]
    private RectTransform entries;

    /// <summary>Keep the authored top edges fixed when text increases content height.</summary>
    protected override void Awake()
    {
        base.Awake();

        if (!body || !entries)
            return;

        RectTransform current = body.rectTransform;
        while (current)
        {
            Vector3[] corners = new Vector3[4];
            current.GetWorldCorners(corners);
            float height = current.rect.height;
            Vector2 minimum = current.anchorMin;
            Vector2 maximum = current.anchorMax;
            minimum.y = 1f;
            maximum.y = 1f;
            current.anchorMin = minimum;
            current.anchorMax = maximum;
            current.pivot = new Vector2(current.pivot.x, 1f);
            current.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Vector3 position = current.position;
            position.y = corners[1].y;
            current.position = position;

            if (current == entries)
                break;

            current = current.parent as RectTransform;
        }
    }

    /// <summary>
    /// Keep text, entry background and scroll bounds aligned after VM updates.
    /// </summary>
    private void LateUpdate()
    {
        if (body && expanded && entry && entries)
        {
            float height = Mathf.Max(46f, body.preferredHeight);
            body.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            expanded.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + 40f);
            float entryHeight = expanded.gameObject.activeSelf ? height + 150f : 100f;
            entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, entryHeight);
            entries.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, entry.gameObject.activeSelf ? entryHeight : 0f);
        }
    }
}
