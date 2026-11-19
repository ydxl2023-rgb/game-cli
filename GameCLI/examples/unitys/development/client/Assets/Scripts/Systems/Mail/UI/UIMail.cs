using System;
using System.Collections;
using Oathx.Widget;
using UnityEngine;

/// <summary>Displays a host-owned inbox through standard Widget bindings.</summary>
[Config("Assets/Data/Widgets/Mail/Mail.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIMail : UIWidget<VMMail>
{
    /// <summary>Optional example inbox, used only before a product supplies data.</summary>
    [SerializeField]
    private TextAsset example;

    /// <summary>Original page entrance and closing states.</summary>
    [SerializeField]
    private Animator entrance;

    /// <summary>Interaction gate while the page closes.</summary>
    [SerializeField]
    private CanvasGroup interaction;

    /// <summary>Restore the original entrance when activated.</summary>
    /// <returns>True when the base widget activated.</returns>
    public override bool Active()
    {
        bool active = base.Active();
        if (active && entrance && interaction)
        {
            interaction.interactable = true;
            entrance.Play("Open", 0, 0f);
        }

        return active;
    }

    /// <summary>Complete framework closure after the source exit animation.</summary>
    /// <param name="complete">Framework completion callback.</param>
    public override void Close(Action complete)
    {
        StartCoroutine(PlayClose(complete));
    }

    /// <summary>Read the optional local example without owning business data.</summary>
    /// <returns>The example JSON or an empty string.</returns>
    public string GetExample()
    {
        return example ? example.text : string.Empty;
    }

    /// <summary>Play the original closing state without allowing another action.</summary>
    /// <param name="complete">Framework completion callback.</param>
    /// <returns>The source-duration close sequence.</returns>
    private IEnumerator PlayClose(Action complete)
    {
        if (interaction)
            interaction.interactable = false;

        if (entrance)
        {
            entrance.Play("Close", 0, 0f);
            yield return new WaitForSecondsRealtime(0.27f);
        }

        complete?.Invoke();
    }
}
