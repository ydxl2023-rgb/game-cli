using Config;

/// <summary>Resolves generated CommonText identifiers through the product language table.</summary>
public static class CommonText
{
    /// <summary>Reads localized content using the development CommonText lookup convention.</summary>
    /// <param name="id">Stable identifier generated from propCommonText.xlsx.</param>
    /// <returns>The language-table text or a source value that does not require localization.</returns>
    public static string Get(int id)
    {
        var entry = Excel.propCommonTextById(id);
        if (int.TryParse(entry.Context, out int languageId))
            return Excel.propLanguageById(languageId);

        return entry.Context;
    }
}
