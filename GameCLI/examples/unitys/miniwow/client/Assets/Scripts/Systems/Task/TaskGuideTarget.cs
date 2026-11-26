using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Registers a serialized UI target independently of any task ID or gameplay system.</summary>
[RequireComponent(typeof(Button))]
public sealed class TaskGuideTarget : MonoBehaviour
{
    /// <summary>Live targets exposed by currently open pages.</summary>
    private static readonly Dictionary<string, TaskGuideTarget> targets =
        new Dictionary<string, TaskGuideTarget>();

    /// <summary>Stable target token referenced by guide expressions.</summary>
    [SerializeField]
    private string targetKey;

    /// <summary>Authored noninteractive finger presentation.</summary>
    [SerializeField]
    private GameObject finger;

    /// <summary>The presentation currently waiting for this button.</summary>
    private TaskFingerAction action;

    /// <summary>Resolves a visible target without searching scene paths.</summary>
    /// <param name="key">Configured target token.</param>
    /// <returns>The currently registered target.</returns>
    public static TaskGuideTarget Resolve(string key)
    {
        if (!targets.TryGetValue(key, out var target))
            throw new InvalidOperationException("Task guide target is not registered: " + key);

        return target;
    }

    /// <summary>Shows the authored finger without invoking the business button.</summary>
    /// <param name="request">The guide waiting for a real click.</param>
    public void Show(TaskFingerAction request)
    {
        if (!GetComponent<Button>().IsInteractable())
            throw new InvalidOperationException("Task guide target is not interactable: " + targetKey);

        action = request;
        finger.SetActive(true);
    }

    /// <summary>Releases only the requesting guide's presentation.</summary>
    /// <param name="request">The owning guide.</param>
    public void Hide(TaskFingerAction request)
    {
        if (action != request)
            return;

        action = null;
        finger.SetActive(false);
    }

    /// <summary>Registers page-owned identity and observes its existing button.</summary>
    private void OnEnable()
    {
        targets.Add(targetKey, this);
        finger.SetActive(false);
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    /// <summary>Unregisters closed targets and releases any pending presentation.</summary>
    private void OnDisable()
    {
        targets.Remove(targetKey);
        GetComponent<Button>().onClick.RemoveListener(OnClicked);
        if (action != null)
            action.OnTargetClosed();
    }

    /// <summary>Completes guidance only after the actual button was clicked.</summary>
    private void OnClicked()
    {
        if (action != null)
            action.Finish(true);
    }
}
