using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Binds formally generated trading assets and migrates hospital assets with their GUIDs.</summary>
[InitializeOnLoad]
public static class MiniwowShopSetup
{
    /// <summary>Explicit local authoring request directory.</summary>
    private const string toolsPath = "../tools/shop-ui/";

    /// <summary>Original trading room skeleton name.</summary>
    private const string spineName = "e_ui_bg_trading_1";

    /// <summary>Trading room dependencies owned by Shop.</summary>
    private const string spinePath = "Assets/Data/Widgets/Shop/Spine/";

    /// <summary>Registers the explicit edit-mode authoring request.</summary>
    static MiniwowShopSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Handles an explicit request only when asset editing is safe.</summary>
    private static void Poll()
    {
        string request = toolsPath + "bind.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
            || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        File.Delete(request);
        try
        {
            MigrateHospital();
            ImportIcons();
            ImportBorders();
            var skeleton = ImportInterior();
            ConfigurePrefab("Assets/Data/Widgets/Common/Micros/UIShopOffer.prefab", false);
            BindOfferGuide();
            BindShop(skeleton);
            ConfigurePrefab("Assets/Data/Widgets/Shop/UIShopPurchase.prefab", true);
            AssetDatabase.SaveAssets();
            File.WriteAllText(toolsPath + "bind-result.json", "{\"ok\":true}");
        }
        catch (System.Exception exception)
        {
            File.WriteAllText(toolsPath + "bind-result.json", new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Binds the original contextual purchase finger to the real offer button.</summary>
    private static void BindOfferGuide()
    {
        const string path = "Assets/Data/Widgets/Common/Micros/UIShopOffer.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var button = root.GetComponentInChildren<Button>(true);
            var target = button.GetComponent<TaskGuideTarget>();
            if (!target)
                target = button.gameObject.AddComponent<TaskGuideTarget>();

            target.enabled = false;
            var marker = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Data/Widgets/Task/UITaskGuideFinger.prefab");
            var existing = button.transform.Find(marker.name);
            var finger = existing ? existing.gameObject
                : (GameObject)PrefabUtility.InstantiatePrefab(marker, button.transform);
            var rect = (RectTransform)finger.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            var icon = (RectTransform)rect.GetChild(0);
            icon.anchorMin = new Vector2(0.5f, 0.5f);
            icon.anchorMax = icon.anchorMin;
            icon.pivot = new Vector2(0f, 1f);
            icon.anchoredPosition = Vector2.zero;
            icon.localRotation = Quaternion.Euler(0f, 0f, 90f);
            icon.sizeDelta = new Vector2(100f, 100f);
            foreach (var graphic in finger.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;

            finger.SetActive(false);
            var definition = JObject.Parse(File.ReadAllText(toolsPath + "guide-target.json"));
            var binding = new SerializedObject(target);
            binding.FindProperty("targetKey").stringValue = (string)definition["key"];
            binding.FindProperty("finger").objectReferenceValue = finger;
            binding.ApplyModifiedPropertiesWithoutUndo();
            var card = new SerializedObject(root.GetComponent<UIShopOffer>());
            card.FindProperty("guideTarget").objectReferenceValue = target;
            card.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Moves only hospital-owned code and assets while preserving existing references.</summary>
    private static void MigrateHospital()
    {
        Move("Assets/Scripts/Systems/Home/UI/UIHomeHospital.cs",
            "Assets/Scripts/Systems/Hospital/UI/UIHomeHospital.cs");
        Move("Assets/Scripts/Systems/Home/VM/VMHomeHospital.cs",
            "Assets/Scripts/Systems/Hospital/VM/VMHomeHospital.cs");
        Move("Assets/Data/Widgets/Home/UIHomeHospital.prefab", "Assets/Data/Widgets/Hospital/UIHomeHospital.prefab");
        Move("Assets/Data/Widgets/Home/Hospital", "Assets/Data/Widgets/Hospital/Resources");
    }

    /// <summary>Performs an idempotent native asset move without replacing the destination.</summary>
    /// <param name="source">Existing source asset.</param>
    /// <param name="destination">New owning system path.</param>
    private static void Move(string source, string destination)
    {
        if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(source)))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        AssetDatabase.Refresh();
        string error = AssetDatabase.MoveAsset(source, destination);
        if (!string.IsNullOrEmpty(error))
            throw new System.InvalidOperationException(error);
    }

    /// <summary>Imports configured item images as shared sprites.</summary>
    private static void ImportIcons()
    {
        var data = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/ShopGoodsConfig.bytes"));
        foreach (var item in ((JObject)data["data"]).Properties())
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath((string)item.Value["IconPath"]);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    /// <summary>Installs the real ScrollRect, reusable Widget template and original room animation.</summary>
    /// <param name="skeleton">Original trading room skeleton.</param>
    private static void BindShop(SkeletonDataAsset skeleton)
    {
        const string path = "Assets/Data/Widgets/Shop/UIShop.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            ConfigureGraphics(root);
            var nodes = root.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            nodes["Gold"].gameObject.SetActive(false);
            nodes["TierVisible"].gameObject.SetActive(false);
            nodes["MarketPanel"].GetComponent<Image>().pixelsPerUnitMultiplier = 0.85f;
            nodes["Title"].GetComponent<Text>().color = new Color32(255, 220, 0, 255);
            nodes["Title"].GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            nodes["NpcTitle"].GetComponent<Text>().color = new Color32(65, 179, 53, 255);
            foreach (string name in new[] { "MaterialClick", "EquipmentClick" })
                nodes[name].GetComponent<Image>().color = Color.clear;

            var viewport = (RectTransform)nodes["Viewport"];
            var content = (RectTransform)nodes["Content"];
            var scroll = nodes["Offers"].GetComponent<ScrollRect>();
            if (!scroll)
                scroll = nodes["Offers"].gameObject.AddComponent<ScrollRect>();

            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            if (!viewport.GetComponent<RectMask2D>())
                viewport.gameObject.AddComponent<RectMask2D>();

            var hit = viewport.GetComponent<Image>();
            if (!hit)
                hit = viewport.gameObject.AddComponent<Image>();

            hit.color = Color.clear;
            hit.raycastTarget = true;
            var layout = content.GetComponent<HorizontalLayoutGroup>();
            if (!layout)
                layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();

            layout.spacing = 10f;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 650f);
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (!fitter)
                fitter = content.gameObject.AddComponent<ContentSizeFitter>();

            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            foreach (var old in viewport.GetComponentsInChildren<UIShopOffer>(true))
                Object.DestroyImmediate(old.gameObject);

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Data/Widgets/Common/Micros/UIShopOffer.prefab");
            var template = (GameObject)PrefabUtility.InstantiatePrefab(asset, viewport);
            template.name = "OfferTemplate";
            template.SetActive(false);
            if (!nodes["Room"].GetComponent<RectMask2D>())
                nodes["Room"].gameObject.AddComponent<RectMask2D>();

            AddInterior(nodes["RoomSpine"], skeleton);
            AddPlayer(nodes["Player"]);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Configures popup raycasts and its full-screen modal shade.</summary>
    /// <param name="path">Formally generated popup.</param>
    /// <param name="modal">Whether to configure a full-screen modal shade.</param>
    private static void ConfigurePrefab(string path, bool modal)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            ConfigureGraphics(root);
            if (!modal)
            {
                var owned = root.GetComponentsInChildren<Text>(true).Single(item => item.name == "Owned");
                owned.gameObject.SetActive(false);
                var rect = (RectTransform)root.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(260f, 650f);
                rect.localScale = Vector3.one;
            }

            if (modal)
            {
                var shade = root.GetComponentsInChildren<Image>(true).Single(image => image.name == "Shade");
                shade.sprite = null;
                shade.color = new Color(0f, 0f, 0f, 0.65f);
                shade.raycastTarget = true;
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Restores original slice borders for the subset of shared images used by trading.</summary>
    private static void ImportBorders()
    {
        var rows = JArray.Parse(File.ReadAllText("../tools/hospital-ui/slice-borders.json"));
        rows.Merge(JArray.Parse(File.ReadAllText(toolsPath + "slice-borders.json")));
        foreach (JObject row in rows)
        {
            string path = "Assets/Data/Widgets/Shop/Images/" + (string)row["name"] + ".png";
            if (!File.Exists(path))
                continue;

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var border = row["border"];
            importer.spriteBorder = new Vector4((float)border[0], (float)border[1],
                (float)border[2], (float)border[3]);
            importer.SaveAndReimport();
        }
    }

    /// <summary>Restores sliced panels and leaves decorative graphics transparent to input.</summary>
    /// <param name="root">The current prefab root.</param>
    private static void ConfigureGraphics(GameObject root)
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Fonts/hanyijingkai.ttf");
        Color textColor = root.GetComponent<UIShopPurchase>()
            ? new Color32(62, 44, 30, 255) : new Color32(247, 231, 196, 255);
        foreach (var text in root.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
            text.color = textColor;
        }

        foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = graphic.GetComponent<Button>() || graphic.name == "Background";

        var buttonFrame = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Data/Widgets/Shop/Images/common_bg_11.png");
        foreach (var image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite && image.sprite.name == "cons_bg_7")
                image.sprite = buttonFrame;

            if (image.sprite && image.sprite.border != Vector4.zero)
                image.type = Image.Type.Sliced;
        }
    }

    /// <summary>Fits the original animated room to the portrait upper panel without stretching.</summary>
    /// <param name="node">Authored Spine mount.</param>
    /// <param name="data">Imported original skeleton.</param>
    private static void AddInterior(Transform node, SkeletonDataAsset data)
    {
        var graphic = node.GetComponent<SkeletonGraphic>();
        if (!graphic)
            graphic = node.gameObject.AddComponent<SkeletonGraphic>();

        graphic.skeletonDataAsset = data;
        graphic.material = AssetDatabase.LoadAssetAtPath<Material>(spinePath + "Interior.mat");
        graphic.startingAnimation = ActorAnimationNames.idle;
        graphic.startingLoop = true;
        graphic.allowMultipleCanvasRenderers = true;
        graphic.raycastTarget = false;
        graphic.Initialize(true);
        float[] vertices = null;
        graphic.Skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertices);
        float scale = Mathf.Max(1080f / (width * 100f), 730f / (height * 100f));
        node.localScale = Vector3.one * scale;
        var rect = (RectTransform)node;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.anchoredPosition = new Vector2(540f - (x + width * 0.5f) * scale * 100f,
            -(y + height) * scale * 100f);
    }

    /// <summary>Restores the original playable character inside the trading room.</summary>
    /// <param name="node">Formally generated character mount.</param>
    private static void AddPlayer(Transform node)
    {
        var data = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
            "Assets/Data/Scenes/Town/Spine/role_01/Skeleton.asset");
        var graphic = node.GetComponent<SkeletonGraphic>();
        if (!graphic)
            graphic = node.gameObject.AddComponent<SkeletonGraphic>();

        graphic.skeletonDataAsset = data;
        graphic.material = AssetDatabase.LoadAssetAtPath<Material>(spinePath + "Interior.mat");
        graphic.startingAnimation = ActorAnimationNames.idle;
        graphic.startingLoop = true;
        graphic.allowMultipleCanvasRenderers = true;
        graphic.raycastTarget = false;
        graphic.Initialize(true);
        graphic.Update(0f);
        float[] vertices = null;
        graphic.Skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertices);
        float scale = 210f / (height * 100f);
        node.localScale = Vector3.one * scale;
        var rect = (RectTransform)node;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.anchoredPosition = new Vector2(400f - (x + width * 0.5f) * scale * 100f,
            -590f - y * scale * 100f);
    }

    /// <summary>Imports the original skeleton, atlas and texture as persistent Unity assets.</summary>
    /// <returns>The hospital interior skeleton asset.</returns>
    private static SkeletonDataAsset ImportInterior()
    {
        string texturePath = spinePath + spineName + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType = TextureImporterType.Default;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        string materialPath = spinePath + "Interior.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (!material)
        {
            material = new Material(Shader.Find("Spine/SkeletonGraphic"));
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        var atlas = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(spinePath + "Atlas.asset");
        if (!atlas)
        {
            atlas = SpineAtlasAsset.CreateRuntimeInstance(
                AssetDatabase.LoadAssetAtPath<TextAsset>(spinePath + spineName + ".atlas.txt"),
                new[] { material }, true);
            AssetDatabase.CreateAsset(atlas, spinePath + "Atlas.asset");
        }

        var skeleton = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(spinePath + "Skeleton.asset");
        if (!skeleton)
        {
            skeleton = SkeletonDataAsset.CreateRuntimeInstance(
                AssetDatabase.LoadAssetAtPath<TextAsset>(spinePath + spineName + ".skel.bytes"), atlas, true);
            skeleton.scale = 1f;
            AssetDatabase.CreateAsset(skeleton, spinePath + "Skeleton.asset");
        }

        return skeleton;
    }

}
