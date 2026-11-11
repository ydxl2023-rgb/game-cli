using System;
using Config;

/// <summary>Resolves fixed UI text through CommonText and the active language table.</summary>
public static class CommonText
{
    /// <summary>Read a localized CommonText entry and substitute its placeholders.</summary>
    /// <param name="id">Stable business identifier generated from propCommonText.</param>
    /// <param name="args">Values for the complete phrase's format placeholders.</param>
    /// <returns>The localized and formatted display text.</returns>
    public static string Get(int id, params object[] args)
    {
        var entry = Excel.propCommonTextById(id);
        if (entry == null)
            throw new InvalidOperationException($"CommonText entry {id} is missing.");

        string text = entry.Context;
        if (int.TryParse(text, out int languageId))
            text = Excel.propLanguageById(languageId);

        if (text == null)
            throw new InvalidOperationException($"CommonText entry {id} has no language text.");

        if (args == null || args.Length == 0)
            return text;

        return string.Format(text, args);
    }
}
