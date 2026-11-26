using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Finishes native bindings on the formally imported portrait entry prefab.</summary>
public static class MiniwowLoginSetup
{
    /// <summary>Enables the project's required legacy UGUI text mode without replacing other defines.</summary>
    public static void EnableLegacyText()
    {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (!System.Array.Exists(defines.Split(';'), value => value == "USE_OLD_TEXT"))
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.IsNullOrEmpty(defines) ? "USE_OLD_TEXT" : defines + ";USE_OLD_TEXT");
    }

    /// <summary>Preserves the existing art and binds fonts, raycasts and portrait anchors.</summary>
    public static void Bind()
    {
        const string path = "Assets/Data/Widgets/Login/UILogin.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Fonts/fangzhenglanting.ttf");
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.raycastTarget = false;
            }

            RectTransform background = (RectTransform)root.transform.Find("Background");
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.one;
            background.pivot = new Vector2(0.5f, 0.5f);
            background.offsetMin = background.offsetMax = Vector2.zero;
            var fit = background.GetComponent<AspectRatioFitter>() ?? background.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 720f / 1560f;
            background.GetComponent<Image>().preserveAspect = false;
            background.GetComponent<Image>().raycastTarget = true;

            RectTransform panel = (RectTransform)root.transform.Find("EntryPanel");
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = panel.offsetMax = Vector2.zero;
            foreach (RectTransform child in panel)
            {
                Vector2 prior = child.anchoredPosition;
                Vector2 anchor = child.anchorMin;
                bool top = child.name == "Logo" || child.name == "AgeRating" || child.name == "Notice";
                float verticalAnchor = top ? 1f : 0f;
                child.anchorMin = child.anchorMax = new Vector2(0.5f, verticalAnchor);
                child.pivot = new Vector2(0.5f, 0.5f);
                child.anchoredPosition = new Vector2(
                    prior.x + 1080f * (anchor.x - 0.5f),
                    prior.y + 1920f * (anchor.y - verticalAnchor));
            }

            panel.Find("LoadingVisible").gameObject.SetActive(false);
            panel.Find("FailedVisible").gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
