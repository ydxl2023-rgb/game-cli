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
