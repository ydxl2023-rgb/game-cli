using Oathx.Widget;
using System.Collections;
using UnityEngine;

/// <summary>Displays the Shop view through serialized Metadata.</summary>
[Config("Assets/Data/Widgets/Shop/Shop.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIShop : UIWidget<VMShop>
{
    /// <summary>Blocks mall input during the original opening interval.</summary>
    [SerializeField]
    private CanvasGroup interaction;

    /// <summary>Read the source opening gate for separately hosted product pages.</summary>
    /// <returns>True after the mall entrance allows input.</returns>
    public bool CanInteract()
    {
        return interaction && interaction.interactable;
    }

    /// <summary>Restart the original half-second opening interaction gate.</summary>
    private void OnEnable()
    {
        if (interaction)
        {
            interaction.interactable = false;

            StartCoroutine(EnableInteraction());
        }
    }

    /// <summary>Release the gate when the view is hidden or destroyed.</summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        if (interaction)
            interaction.interactable = true;
    }

    /// <summary>Restore input after the source opening gate duration.</summary>
    /// <returns>The unscaled opening delay.</returns>
    private IEnumerator EnableInteraction()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (interaction)
            interaction.interactable = true;
    }
}
