using Oathx.Widget;
using UnityEngine;

/// <summary>
/// Displays caller-supplied loading progress without performing loading work.
/// </summary>
[Config("Assets/Data/Widgets/Loading/Loading.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UILoading : UIWidget<VMLoading>
{
    /// <summary>
    /// Display the normalized progress and its current status message.
    /// </summary>
    /// <param name="progress">Progress from zero to one; NaN is treated as zero.</param>
    /// <param name="message">Status message preceding the percentage.</param>
    /// <returns>True when the bound view model was updated.</returns>
    public bool UpdateProgress(float progress, string message = "Loading...")
    {
        if (!vm)
            return false;

        progress = float.IsNaN(progress) ? 0f : Mathf.Clamp01(progress);
        vm.Rate = $"{message ?? string.Empty}({Mathf.RoundToInt(progress * 100f)}%)";
        return true;
    }

    /// <summary>
    /// Update the additional loading tip.
    /// </summary>
    /// <param name="text">Tip to display; null clears the label.</param>
    /// <returns>True when the bound view model was updated.</returns>
    public bool SetTip(string text)
    {
        if (!vm)
            return false;

        vm.TipText = text ?? string.Empty;
        return true;
    }
}
