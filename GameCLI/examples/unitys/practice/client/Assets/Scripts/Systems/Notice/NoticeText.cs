using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UGUI text with HTTP links whose hit areas follow the generated glyph geometry.
/// </summary>
public class NoticeText : Text, IPointerDownHandler, IPointerClickHandler
{
    /// <summary>
    /// Display color shared by valid links.
    /// </summary>
    [SerializeField]
    private Color linkColor = new Color(0.12f, 0.46f, 0.78f, 1f);

    /// <summary>
    /// Supported anchors with quoted or unquoted destinations.
    /// </summary>
    private static readonly Regex anchors = new Regex(
        @"<a\s+href\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>]+))\s*>(.*?)</a\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Color tags are replaced inside links to keep link geometry identifiable.
    /// </summary>
    private static readonly Regex colors = new Regex(@"</?color(?:=[^>]+)?>", RegexOptions.IgnoreCase);

    /// <summary>
    /// Links belonging to the current text value.
    /// </summary>
    private readonly List<(string url, Color32 marker, List<Rect> bounds)> links = new List<(string, Color32, List<Rect>)>();

    /// <summary>
    /// Original input used to detect unchanged assignments.
    /// </summary>
    private string source;

    /// <summary>
    /// Pointer position at the beginning of a possible click.
    /// </summary>
    private Vector2 pressed;

    /// <summary>
    /// Parse anchor markup while keeping standard Text bindings compatible.
    /// </summary>
    public override string text
    {
        get { return base.text; }
        set
        {
            string input = value ?? string.Empty;
            if (source != input)
            {
                source = input;
                links.Clear();
                base.text = anchors.Replace(input, match => {
                    string destination = WebUtility.HtmlDecode(match.Groups[1].Success ? match.Groups[1].Value
                        : match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value);
                    string label = match.Groups[4].Value;
                    if (TryNormalizeUrl(destination, out string url))
                    {
                        int index = links.Count + 1;
                        var marker = new Color32(1, (byte)(index >> 8), (byte)index, 255);
                        links.Add((url, marker, new List<Rect>()));

                        return "<color=#" + ColorUtility.ToHtmlStringRGBA(marker) + ">" + colors.Replace(label, string.Empty) + "</color>";
                    }

                    return label;
                });
                supportRichText = true;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    /// <summary>
    /// Remember the start of a click without taking ownership of ScrollRect dragging.
    /// </summary>
    /// <param name="data">Pointer press information.</param>
    public void OnPointerDown(PointerEventData data)
    {
        pressed = data.position;
    }

    /// <summary>
    /// Open a link only for a completed, stationary left-button click.
    /// </summary>
    /// <param name="data">Pointer click information.</param>
    public void OnPointerClick(PointerEventData data)
    {
        float threshold = EventSystem.current ? EventSystem.current.pixelDragThreshold : 10f;
        if (data.button == PointerEventData.InputButton.Left && !data.dragging && data.eligibleForClick
            && (data.position - pressed).sqrMagnitude <= threshold * threshold
            && TryGetLink(data.position, data.pressEventCamera, out string url))
        {
            Application.OpenURL(url);
        }
    }

    /// <summary>
    /// Resolve a visible hyperlink at a screen point, respecting parent masks.
    /// </summary>
    /// <param name="point">Screen-space pointer position.</param>
    /// <param name="camera">Event camera, or null for an overlay canvas.</param>
    /// <param name="url">Normalized destination when a link was hit.</param>
    /// <returns>True when the point is inside a visible link glyph.</returns>
    public bool TryGetLink(Vector2 point, Camera camera, out string url)
    {
        url = string.Empty;
        if (!IsActive() || !Raycast(point, camera)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, point, camera, out Vector2 local))
            return false;

        foreach (var link in links)
        {
            if (link.bounds.Exists(rect => rect.Contains(local)))
            {
                url = link.url;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Accept absolute HTTP and HTTPS destinations without executable URI schemes.
    /// </summary>
    /// <param name="value">Destination supplied by the announcement.</param>
    /// <param name="url">Normalized browser destination.</param>
    /// <returns>True for a supported absolute web address.</returns>
    public static bool TryNormalizeUrl(string value, out string url)
    {
        url = string.Empty;
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrEmpty(uri.Host))
            return false;

        url = uri.AbsoluteUri;

        return true;
    }

    /// <summary>
    /// Build per-glyph hit boxes after Unity has performed wrapping and rich-text layout.
    /// </summary>
    /// <param name="mesh">Generated Text mesh.</param>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        base.OnPopulateMesh(mesh);
        foreach (var link in links)
            link.bounds.Clear();

        var vertex = new UIVertex();
        for (int index = 0; index + 3 < mesh.currentVertCount; index += 4)
        {
            mesh.PopulateUIVertex(ref vertex, index);
            var link = links.Find(item => item.marker.Equals(vertex.color));
            if (link.bounds != null)
            {
                Vector2 min = vertex.position;
                Vector2 max = vertex.position;
                for (int corner = 0; corner < 4; corner++)
                {
                    mesh.PopulateUIVertex(ref vertex, index + corner);
                    min = Vector2.Min(min, vertex.position);
                    max = Vector2.Max(max, vertex.position);
                    vertex.color = linkColor;
                    mesh.SetUIVertex(vertex, index + corner);
                }
                if (max.x > min.x && max.y > min.y)
                    link.bounds.Add(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
            }
        }
    }

}
