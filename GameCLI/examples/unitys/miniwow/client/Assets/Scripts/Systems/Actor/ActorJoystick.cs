using UnityEngine;

/// <summary>Displays fixed-position analog input without changing actor or camera state.</summary>
public sealed class ActorJoystick : MonoBehaviour
{
    /// <summary>Full-screen coordinate space authored by the UGUI importer.</summary>
    [SerializeField]
    private RectTransform surface;

    /// <summary>Authored fixed visual root.</summary>
    [SerializeField]
    private RectTransform stick;

    /// <summary>Movable original joystick image.</summary>
    [SerializeField]
    private RectTransform knob;

    /// <summary>Original direction marker whose pivot follows the player's feet.</summary>
    [SerializeField]
    private RectTransform directionMarker;

    /// <summary>Configured radius in design canvas units.</summary>
    private float radius;

    /// <summary>Initial pointer position in canvas units.</summary>
    private Vector2 origin;

    /// <summary>Validates references and installs the design-space travel radius.</summary>
    /// <param name="value">Radius in original FairyGUI units.</param>
    /// <param name="sourceWidth">Original UI design width.</param>
    /// <returns>Whether the component can display input.</returns>
    public bool Initialize(float value, float sourceWidth)
    {
        if (!surface || !stick || !knob || !directionMarker || value <= 0f || sourceWidth <= 0f)
            return false;

        radius = value * surface.rect.width / sourceWidth;
        Hide();

        return true;
    }

    /// <summary>Captures the drag origin without moving the authored visual root.</summary>
    /// <param name="position">Screen pointer position.</param>
    public void Begin(Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(surface, position, null, out origin);
        knob.anchoredPosition = Vector2.zero;
        stick.gameObject.SetActive(true);
    }

    /// <summary>Updates the knob and returns bounded analog intent.</summary>
    /// <param name="position">Current screen pointer position.</param>
    /// <returns>Direction with strength from zero to one.</returns>
    public Vector2 Move(Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(surface, position, null, out Vector2 local);
        Vector2 delta = Vector2.ClampMagnitude(local - origin, radius);
        knob.anchoredPosition = delta;
        directionMarker.gameObject.SetActive(delta.sqrMagnitude > 0f);
        directionMarker.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg);

        return delta / radius;
    }

    /// <summary>Positions the original arrow relative to the logical player's screen location.</summary>
    /// <param name="position">Authoritative player position.</param>
    /// <param name="camera">Scene projection camera.</param>
    public void Follow(Vector3 position, Camera camera)
    {
        Vector2 screen = camera.WorldToScreenPoint(position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(surface, screen, null, out Vector2 local);
        directionMarker.anchoredPosition = local;
    }

    /// <summary>Centers the knob while keeping the fixed joystick visible.</summary>
    public void ResetInput()
    {
        if (knob)
            knob.anchoredPosition = Vector2.zero;

        if (directionMarker)
            directionMarker.gameObject.SetActive(false);
    }

    /// <summary>Shows the fixed control only while its owning page accepts input.</summary>
    /// <param name="visible">Whether the control should be visible.</param>
    public void SetVisible(bool visible)
    {
        ResetInput();

        if (stick)
            stick.gameObject.SetActive(visible);
    }

    /// <summary>Clears the presentation without owning logical input state.</summary>
    public void Hide()
    {
        SetVisible(false);
    }
}
