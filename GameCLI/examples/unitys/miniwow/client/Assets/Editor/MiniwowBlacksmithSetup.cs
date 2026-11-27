using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Binds formally imported forge widgets with native Unity references.</summary>
[InitializeOnLoad]
public static class MiniwowBlacksmithSetup
{
    /// <summary>Explicit edit-mode request directory.</summary>
    private const string toolsPath = "../tools/blacksmith-ui/";

    /// <summary>Registers the native binding request.</summary>
    static MiniwowBlacksmithSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Consumes requests only when asset editing is safe.</summary>
    private static void OnUpdate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
            || EditorApplication.isUpdating || !File.Exists(toolsPath + "bind.request"))
            return;

        File.Delete(toolsPath + "bind.request");
        try
        {
            ImportSprites();
            Configure("Assets/Data/Widgets/Common/Micros/UIBlacksmithEntry.prefab", false, true);
            Configure("Assets/Data/Widgets/Blacksmith/UIBlacksmithConfirm.prefab", true, false);
            Configure("Assets/Data/Widgets/Blacksmith/UIBlacksmith.prefab", false, false);
            AssetDatabase.SaveAssets();
            File.WriteAllText(toolsPath + "bind-result.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText(toolsPath + "bind-result.json",
                new JObject { ["ok"] = false, ["error"] = exception.ToString() }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Imports tracked image assets and recovered nine-slice borders.</summary>
    private static void ImportSprites()
    {
        var rows = JArray.Parse(File.ReadAllText(toolsPath + "imports.json"));
        var borders = JArray.Parse(File.ReadAllText("../tools/hospital-ui/slice-borders.json"));
        foreach (string path in rows.Select(row => (string)row["destination"]).Distinct())
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            var border = borders.FirstOrDefault(row => (string)row["name"] == Path.GetFileNameWithoutExtension(path));
            if (border != null)
            {
                var values = border["border"];
                importer.spriteBorder = new Vector4((float)values[0], (float)values[1],
                    (float)values[2], (float)values[3]);
            }

            importer.SaveAndReimport();
        }
    }

    /// <summary>Configures raycasts, modal shade and list infrastructure.</summary>
    /// <param name="path">Authored prefab path.</param>
    /// <param name="modal">Whether this is a confirmation popup.</param>
    /// <param name="cell">Whether this is a reusable row.</param>
    private static void Configure(string path, bool modal, bool cell)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = graphic.GetComponent<Button>();

            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite && image.sprite.border != Vector4.zero)
                    image.type = Image.Type.Sliced;
            }

            if (cell)
            {
                var rect = (RectTransform)root.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(1020f, 195f);
                rect.localScale = Vector3.one;
            }
            else
            {
                BindAnchors(root, modal);
                string shadeName = modal ? "Shade" : "InputBlock";
                var shade = root.GetComponentsInChildren<Image>(true).Single(item => item.name == shadeName);
                shade.sprite = null;
                shade.color = new Color(0f, 0f, 0f, modal ? 0.65f : 0f);
                shade.raycastTarget = true;
                shade.rectTransform.anchorMin = Vector2.zero;
                shade.rectTransform.anchorMax = Vector2.one;
                shade.rectTransform.offsetMin = Vector2.zero;
                shade.rectTransform.offsetMax = Vector2.zero;
                if (!modal)
                    BindList(root);
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Preserves authored section offsets across portrait aspect ratios.</summary>
    /// <param name="root">Formally generated page.</param>
    /// <param name="modal">Whether the page is a centered confirmation.</param>
    private static void BindAnchors(GameObject root, bool modal)
    {
        var definition = JObject.Parse(File.ReadAllText(toolsPath + root.name + ".json"));
        var rows = definition["nodes"][0]["children"];
        foreach (var row in rows)
        {
            string name = (string)row["name"];
            var rect = (RectTransform)root.transform.Find(name);
            var source = row["rect"];
            if (row["text"] != null)
                rect.GetComponent<Text>().text = (string)row["text"]["content"];

            bool bottom = name == "Footer" || name == "RecipesClick" || name == "ShelfClick"
                || name == "HeroClick" || name == "Feedback";
            float anchor = modal ? 0.5f : bottom ? 0f : 1f;
            rect.anchorMin = new Vector2(0f, anchor);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2((float)source["width"], (float)source["height"]);
            rect.anchoredPosition = new Vector2((float)source["x"],
                (1f - anchor) * 1920f - (float)source["y"]);
            if (name == "Background")
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = new Vector2(0f, -570f);
            }
        }
    }

    /// <summary>Installs the framework-owned list and reusable inactive template.</summary>
    /// <param name="root">Forge page instance.</param>
    private static void BindList(GameObject root)
    {
        foreach (var old in root.GetComponentsInChildren<UIBlacksmithEntry>(true))
            UnityEngine.Object.DestroyImmediate(old.gameObject);

        var nodes = root.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
        var viewport = (RectTransform)nodes["Viewport"];
        var content = (RectTransform)nodes["Content"];
        var scroll = nodes["Entries"].GetComponent<ScrollRect>();
        if (!scroll)
            scroll = nodes["Entries"].gameObject.AddComponent<ScrollRect>();

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        if (!viewport.GetComponent<RectMask2D>())
            viewport.gameObject.AddComponent<RectMask2D>();

        var image = viewport.GetComponent<Image>();
        if (!image)
            image = viewport.gameObject.AddComponent<Image>();

        image.color = Color.clear;
        image.raycastTarget = true;
        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (!layout)
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.spacing = 12f;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 750f);
        var fitter = content.GetComponent<ContentSizeFitter>();
        if (!fitter)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Data/Widgets/Common/Micros/UIBlacksmithEntry.prefab");
        var template = (GameObject)PrefabUtility.InstantiatePrefab(asset, viewport);
        template.name = "EntryTemplate";
        template.SetActive(false);
        var gold = nodes["Gold"].GetComponent<Text>();
        gold.color = new Color32(247, 231, 196, 255);
        nodes["Title"].GetComponent<Text>().color = gold.color;
    }
}
