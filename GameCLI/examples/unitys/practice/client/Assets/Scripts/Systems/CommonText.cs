using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>Reads the practice source-language table exported from propCommonText.</summary>
public static class CommonText
{
    /// <summary>Resolve a fixed text identifier without embedding UI strings in code.</summary>
    /// <param name="id">Generated CommonText identifier.</param>
    /// <param name="args">Optional phrase substitutions.</param>
    /// <returns>The source-language phrase.</returns>
    public static string Get(int id, params object[] args)
    {
        var resource = Resources.Load<TextAsset>("PracticeCommonText");
        if (!resource)
            throw new InvalidOperationException("Practice CommonText export is missing.");

        var entries = JsonConvert.DeserializeObject<Dictionary<int, string>>(resource.text);
        if (entries == null || !entries.TryGetValue(id, out string text))
            throw new InvalidOperationException($"CommonText entry {id} is missing.");

        if (args == null || args.Length == 0)
            return text;

        return string.Format(text, args);
    }
}
