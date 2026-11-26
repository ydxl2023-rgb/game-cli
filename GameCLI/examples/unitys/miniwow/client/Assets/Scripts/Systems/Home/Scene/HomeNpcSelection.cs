using UnityEngine;

/// <summary>Projects an available resident's original entrance selection without owning business state.</summary>
[System.Serializable]
public sealed class HomeNpcSelection
{
    /// <summary>Original building configuration identity.</summary>
    [SerializeField]
    private int buildingId;

    /// <summary>Original entrance button mount.</summary>
    [SerializeField]
    private Transform entryAnchor;

    /// <summary>Resident visibility controlled by existing scene progression.</summary>
    [SerializeField]
    private GameObject occupant;

    /// <summary>Original building title mount, independent of its entrance trigger.</summary>
    [SerializeField]
    private Transform titleAnchor;

    /// <summary>Original entrance transform used for its oriented trigger volume.</summary>
    [SerializeField]
    private Transform entrance;

    /// <summary>Original collider center in entrance-local coordinates.</summary>
    [SerializeField]
    private Vector3 center;

    /// <summary>Original collider dimensions in entrance-local coordinates.</summary>
    [SerializeField]
    private Vector3 size;

    /// <summary>Original world-space selection presentation.</summary>
    [SerializeField]
    private GameObject effect;

    /// <summary>Reports whether the resident exists in the current scene progression.</summary>
    /// <returns>Whether its title may be displayed.</returns>
    public bool IsVisible()
    {
        return occupant.activeInHierarchy;
    }

    /// <summary>Returns the original title position with its authored presentation offset.</summary>
    /// <returns>The title position in world space.</returns>
    public Vector3 GetTitlePosition()
    {
        return titleAnchor.position + Vector3.down * 0.5f;
    }

    /// <summary>Matches the authored building identity.</summary>
    /// <param name="id">Requested building configuration.</param>
    /// <returns>Whether this resident belongs to the building.</returns>
    public bool Matches(int id)
    {
        return buildingId == id;
    }

    /// <summary>Returns the original entrance button mount.</summary>
    /// <returns>The entrance position in world space.</returns>
    public Vector3 GetEntryPosition()
    {
        return entryAnchor.position;
    }

    /// <summary>Applies proximity and resident visibility to the shared selection presentation.</summary>
    /// <param name="position">Authoritative player position.</param>
    /// <param name="radius">Original player agent radius.</param>
    /// <param name="height">Original player agent height above its feet.</param>
    /// <param name="input">Whether Home currently accepts scene interaction.</param>
    public void Refresh(Vector3 position, float radius, float height, bool input)
    {
        bool visible = input && IsVisible() && IsNearby(position, radius, height);
        if (effect.activeSelf != visible)
            effect.SetActive(visible);
    }

    /// <summary>Checks the player capsule against the original oriented entrance volume.</summary>
    /// <param name="position">Authoritative player position.</param>
    /// <param name="radius">Player agent radius.</param>
    /// <param name="height">Player agent height.</param>
    /// <returns>Whether the player overlaps the entrance.</returns>
    public bool IsNearby(Vector3 position, float radius, float height)
    {
        Vector3 capsuleCenter = position + Vector3.up * (height * 0.5f);
        Vector3 local = entrance.InverseTransformPoint(capsuleCenter) - center;
        Vector3 half = size * 0.5f;
        Vector3 closest = new Vector3(Mathf.Clamp(local.x, -half.x, half.x),
            Mathf.Clamp(local.y, -half.y, half.y), Mathf.Clamp(local.z, -half.z, half.z));
        Vector3 distance = entrance.TransformPoint(closest + center) - capsuleCenter;
        distance.y = Mathf.Max(0f, Mathf.Abs(distance.y) - Mathf.Max(0f, height * 0.5f - radius));

        return distance.sqrMagnitude <= radius * radius;
    }

    /// <summary>Clears presentation during input suspension or scene teardown.</summary>
    public void Hide()
    {
        if (effect)
            effect.SetActive(false);
    }
}
