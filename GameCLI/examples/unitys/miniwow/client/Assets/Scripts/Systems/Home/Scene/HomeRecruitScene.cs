using UnityEngine;

/// <summary>Authored original recruitment actors and entrance projections.</summary>
public sealed class HomeRecruitScene : MonoBehaviour
{
    /// <summary>Camera used solely for projection.</summary>
    [SerializeField]
    private Camera sceneCamera;

    /// <summary>Brother and horn interaction mounts in their original order.</summary>
    [SerializeField]
    private Transform[] entrances;

    /// <summary>Configured additional actor presentations.</summary>
    [SerializeField]
    private ActorShape[] recruits;

    /// <summary>Reports whether the owning Home scene currently accepts interactions.</summary>
    /// <returns>Whether no modal or opening sequence owns the input.</returns>
    public bool CanInteract()
    {
        return GetComponent<HomeScene>().AcceptsInput();
    }

    /// <summary>Projects an original scene mount or recruited hero into screen coordinates.</summary>
    /// <param name="index">Brother, horn, Kai or Toby slot.</param>
    /// <returns>Screen position including visibility depth.</returns>
    public Vector3 GetScreenPoint(int index)
    {
        Vector3 point = index < entrances.Length ? entrances[index].position
            : recruits[index - entrances.Length].transform.position + Vector3.up * 2.5f;

        return sceneCamera.WorldToScreenPoint(point);
    }

    /// <summary>Projects entity eligibility without creating or deleting logical actors.</summary>
    /// <param name="index">Additional hero slot.</param>
    /// <param name="visible">Whether its current recruitment phase exposes the actor.</param>
    public void ShowActor(int index, bool visible)
    {
        recruits[index].gameObject.SetActive(visible);
    }
}
