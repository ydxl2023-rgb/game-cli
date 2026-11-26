using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Provides local scene inspection without owning building or player state.</summary>
public sealed class HomeSceneCamera : MonoBehaviour
{
    /// <summary>The authored camera used to project pointer movement.</summary>
    [SerializeField]
    private Camera sceneCamera;

    /// <summary>The authored camera handle translated across the town.</summary>
    [SerializeField]
    private Transform cameraHandle;

    /// <summary>Limits presentation movement around the initial focus.</summary>
    [SerializeField]
    private float movementRadius = 45f;

    /// <summary>Initial presentation position, independent of gameplay state.</summary>
    private Vector3 origin;

    /// <summary>Previous screen position during a valid drag.</summary>
    private Vector2 previous;

    /// <summary>Whether a pointer gesture started outside the UI.</summary>
    private bool dragging;

    /// <summary>Moves the presentation camera using two screen-space points.</summary>
    /// <param name="from">Previous pointer position.</param>
    /// <param name="to">Current pointer position.</param>
    /// <returns>True when both points intersect the town plane.</returns>
    public bool Pan(Vector2 from, Vector2 to)
    {
        if (!sceneCamera || !cameraHandle)
            return false;

        Plane plane = new Plane(Vector3.up, new Vector3(0f, 35.17353f, 0f));
        Ray first = sceneCamera.ScreenPointToRay(from);
        Ray second = sceneCamera.ScreenPointToRay(to);
        if (!plane.Raycast(first, out float firstDistance) || !plane.Raycast(second, out float secondDistance))
            return false;

        Vector3 delta = first.GetPoint(firstDistance) - second.GetPoint(secondDistance);
        Vector3 offset = cameraHandle.position + delta - origin;
        offset.y = 0f;

        cameraHandle.position = origin + Vector3.ClampMagnitude(offset, movementRadius);

        return true;
    }

    /// <summary>Captures the serialized camera setup after scene activation.</summary>
    private void Awake()
    {
        if (!sceneCamera || !cameraHandle)
        {
            enabled = false;

            return;
        }

        origin = cameraHandle.position;
    }

    /// <summary>Handles mouse or single-touch scene dragging and mouse-wheel zoom.</summary>
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                Begin(touch.position, touch.fingerId);
            else if (touch.phase == TouchPhase.Moved && dragging && Input.touchCount == 1)
                Drag(touch.position);
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled || Input.touchCount > 1)
                dragging = false;

            return;
        }

        if (Input.GetMouseButtonDown(0))
            Begin(Input.mousePosition, -1);

        if (Input.GetMouseButton(0) && dragging)
            Drag(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            dragging = false;

        if (!EventSystem.current || !EventSystem.current.IsPointerOverGameObject())
        {
            float distance = Mathf.Clamp(
                -sceneCamera.transform.localPosition.z - Input.mouseScrollDelta.y * 5f,
                100f,
                180f);
            sceneCamera.transform.localPosition = new Vector3(0f, 0f, -distance);
        }
    }

    /// <summary>Clears transient input when this scene is unloaded or disabled.</summary>
    private void OnDisable()
    {
        dragging = false;
    }

    /// <summary>Begins a gesture only when its initial pointer is outside UI controls.</summary>
    /// <param name="position">Screen-space pointer position.</param>
    /// <param name="pointerId">Touch identifier or the mouse identifier.</param>
    private void Begin(Vector2 position, int pointerId)
    {
        dragging = !EventSystem.current || !EventSystem.current.IsPointerOverGameObject(pointerId);
        previous = position;
    }

    /// <summary>Projects movement onto a horizontal plane and clamps the camera handle.</summary>
    /// <param name="position">Current screen-space pointer position.</param>
    private void Drag(Vector2 position)
    {
        Pan(previous, position);

        previous = position;
    }

}
