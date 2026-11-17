using System;
using System.Collections.Generic;
using System.Globalization;
using Config;
using Oathx.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns explicit red dot bindings on a widget without searching its hierarchy.</summary>
public class RedDotScope : MonoBehaviour
{
    /// <summary>Enabled scopes used when the service activates after a widget.</summary>
    private static readonly HashSet<RedDotScope> instances = new HashSet<RedDotScope>();

    /// <summary>Serialized configuration identifiers and display targets.</summary>
    [SerializeField]
    private RedDotBinding[] bindings;

    /// <summary>Read a safe snapshot of enabled scopes for late service activation.</summary>
    /// <returns>The currently enabled scope instances.</returns>
    public static RedDotScope[] Snapshot()
    {
        var result = new RedDotScope[instances.Count];
        instances.CopyTo(result);

        return result;
    }

    /// <summary>Render configured counts; hidden targets do not disable this scope.</summary>
    /// <param name="entity">Calculated state, or null to hide presentation during shutdown.</param>
    /// <returns>True when every serialized binding is valid.</returns>
    public bool Render(RedDotEntity entity)
    {
        if (bindings == null)
            return false;

        bool complete = true;
        foreach (var binding in bindings)
            complete &= binding != null && binding.Render(entity);

        return complete;
    }

    /// <summary>Register this scope independently of target visibility.</summary>
    private void OnEnable()
    {
        Render(null);
        instances.Add(this);

        XPluginUtility.FireEvent(new RedDotScopeEnabled { scope = this });
    }

    /// <summary>Release listeners without clearing the underlying business state.</summary>
    private void OnDisable()
    {
        instances.Remove(this);

        XPluginUtility.FireEvent(new RedDotScopeDisabled { scope = this });
    }
}

/// <summary>Serialized association between a configuration row and its visual.</summary>
[Serializable]
public class RedDotBinding
{
    /// <summary>Stable propRedDot identifier.</summary>
    [SerializeField]
    private int id;

    /// <summary>Existing or product-instantiated visual controlled by this binding.</summary>
    [SerializeField]
    private GameObject target;

    /// <summary>Optional label required by Count display mode.</summary>
    [SerializeField]
    private Text countText;

    /// <summary>Apply state without changing its business source.</summary>
    /// <param name="entity">Current state, or null when the service stops.</param>
    /// <returns>True when the configuration and its required target exist.</returns>
    public bool Render(RedDotEntity entity)
    {
        var configuration = Excel.propRedDotById(id);
        if (!target || configuration == null)
            return false;

        if (configuration.RedDotDisplay == ERedDotDisplay.Count && !countText)
            return false;

        int count = 0;
        if (entity)
            count = entity.GetCount(id);

        if (countText)
            countText.text = count.ToString(CultureInfo.InvariantCulture);

        target.SetActive(configuration.Enabled != 0 && count > 0);

        return true;
    }
}
