using System.IO;
using System.Linq;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Original inn foreground animation names.</summary>
public static class InnRoomAnimations
{
    /// <summary>Counter layer over the innkeeper.</summary>
    public const string counter = "idle2";
}

/// <summary>Binds the formally generated inn page through native Unity asset APIs.</summary>
[InitializeOnLoad]
public static class MiniwowInnSetup
{
    /// <summary>Explicit authoring request directory.</summary>
    private const string directory = "../tools/inn-ui/";

    /// <summary>Registers edit-time authoring requests.</summary>
    static MiniwowInnSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Handles an explicit request only when assets can be safely edited.</summary>
    private static void Poll()
    {
        string request = directory + "bind.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
            || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        File.Delete(request);
        try
        {
            ImportBorders();
            BindPage();
            AssetDatabase.SaveAssets();
            File.WriteAllText(directory + "bind-result.json", "{\"ok\":true}");
        }
        catch (System.Exception exception)
        {
            File.WriteAllText(directory + "bind-result.json", exception.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Restores original nine-slice borders before configuring the page images.</summary>
    private static void ImportBorders()
    {
        var rows = JArray.Parse(File.ReadAllText("../tools/hospital-ui/slice-borders.json"));
        foreach (JObject row in rows)
        {
            string path = "Assets/Data/Widgets/Inn/Resources/" + (string)row["name"] + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var border = row["border"];
            importer.spriteBorder = new Vector4((float)border[0], (float)border[1],
                (float)border[2], (float)border[3]);
            importer.SaveAndReimport();
        }
    }

    /// <summary>Restores original three-layer room composition and authored controls.</summary>
    private static void BindPage()
    {
        var roomData = ImportInterior("e_ui_bg_hotel_1");
        var npcData = ImportInterior("npc_hotel");
        const string path = "Assets/Data/Widgets/Inn/UIInn.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var definition = JObject.Parse(File.ReadAllText(directory + "UIInn.json"));
            var names = definition.SelectTokens("$..name").Select(item => (string)item).ToArray();
            var nodes = root.GetComponentsInChildren<Transform>(true)
                .Where(item => names.Contains(item.name))
                .ToDictionary(item => item.name);
            var pageBackground = (RectTransform)nodes["PageBackground"];
            pageBackground.anchorMin = Vector2.zero;
            pageBackground.anchorMax = Vector2.one;
            pageBackground.offsetMin = Vector2.zero;
            pageBackground.offsetMax = Vector2.zero;
            foreach (string name in new[] { "RestVisible", "RepairVisible", "Energy" })
            {
                var rect = (RectTransform)nodes[name];
                var node = definition.SelectTokens("$..name").Single(item => (string)item == name).Parent.Parent;
                var bounds = node["rect"];
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2((float)bounds["width"], (float)bounds["height"]);
                rect.anchoredPosition = new Vector2((float)bounds["x"], 960f - (float)bounds["y"]);
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Fonts/fangzhenglanting.ttf");
            foreach (var text in root.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.raycastTarget = false;
            }

            foreach (string name in new[] { "Gem", "TaskText" })
                nodes[name].GetComponent<Text>().color = new Color32(247, 231, 196, 255);

            foreach (string name in new[] { "RestLabel", "RepairCost", "ClaimLabel" })
                nodes[name].GetComponent<Text>().color = new Color32(57, 43, 32, 255);

            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                image.raycastTarget = image.GetComponent<Button>() != null || image.name == "PageBackground";
                if (image.sprite && image.sprite.border != Vector4.zero)
                    image.type = Image.Type.Sliced;
            }

            var binding = new SerializedObject(root.GetComponent<UIInn>());
            binding.FindProperty("materialButton").objectReferenceValue = nodes["CraftClick"].GetComponent<Button>();
            binding.FindProperty("flyingMaterial").objectReferenceValue = nodes["FlyingMaterial"];
            binding.FindProperty("productionTarget").objectReferenceValue = nodes["ProductionTarget"];
            binding.ApplyModifiedPropertiesWithoutUndo();
            if (!nodes["Room"].GetComponent<RectMask2D>())
                nodes["Room"].gameObject.AddComponent<RectMask2D>();

            AddSkeleton(nodes["RoomSpine"], roomData, new Vector2(1080f, 650f));
            var room = (RectTransform)nodes["RoomSpine"];
            room.localScale *= 1.2f;
            var roomGraphic = nodes["RoomSpine"].GetComponent<SkeletonGraphic>();
            var background = roomGraphic.Skeleton.Slots.First(slot =>
                slot.Attachment is Spine.MeshAttachment mesh && mesh.Path == "bg1");
            var attachment = (Spine.MeshAttachment)background.Attachment;
            var corners = new float[attachment.WorldVerticesLength];
            attachment.ComputeWorldVertices(background, 0, corners.Length, corners, 0);
            float top = float.MinValue;
            for (int index = 1; index < corners.Length; index += 2)
                top = Mathf.Max(top, corners[index]);
            room.anchoredPosition = new Vector2(540f, -top * room.localScale.x * 100f);
            AddSkeleton(nodes["Innkeeper"], npcData, new Vector2(200f, 220f));
            var keeper = (RectTransform)nodes["Innkeeper"];
            keeper.localScale = room.localScale * (0.85f / 0.641f);
            keeper.anchoredPosition = room.anchoredPosition
                + new Vector2(-0.024f, 0.535f) * (room.localScale.x * 10000f / 0.641f);
            AddSkeleton(nodes["CounterSpine"], roomData, new Vector2(1080f, 650f));
            var counter = nodes["CounterSpine"].GetComponent<SkeletonGraphic>();
            counter.startingAnimation = InnRoomAnimations.counter;
            counter.AnimationState.SetAnimation(0, InnRoomAnimations.counter, true);
            counter.transform.localScale = room.localScale;
            ((RectTransform)counter.transform).anchoredPosition = room.anchoredPosition;
            var hero = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
                "Assets/Data/Scenes/Town/Spine/hero_20025/Skeleton.asset");
            AddSkeleton(nodes["Patient"], hero, new Vector2(110f, 125f));
            nodes["UpgradeClick"].GetComponent<Button>().interactable = false;
            nodes["AutoCraftClick"].GetComponent<Button>().interactable = false;
            nodes["CapacityIcon"].GetComponent<Image>().enabled = true;
            nodes["CraftClick"].GetComponent<Image>().color = Color.clear;
            foreach (var graphic in root.GetComponentsInChildren<SkeletonGraphic>(true))
            {
                graphic.Update(0f);
                graphic.MatchRectTransformWithBounds();
            }

            BindGuide(nodes["CraftClick"].GetComponent<Button>());
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Registers the original material guide identity on its real button.</summary>
    /// <param name="button">Authored recipe material control.</param>
    private static void BindGuide(Button button)
    {
        var marker = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Task/UITaskGuideFinger.prefab");
        var existing = button.transform.Find(marker.name);
        var finger = existing ? existing.gameObject :
            (GameObject)PrefabUtility.InstantiatePrefab(marker, button.transform);
        var rect = (RectTransform)finger.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        foreach (var graphic in finger.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

        finger.SetActive(false);
        var target = button.GetComponent<TaskGuideTarget>();
        if (!target)
            target = button.gameObject.AddComponent<TaskGuideTarget>();

        var serialized = new SerializedObject(target);
        serialized.FindProperty("targetKey").stringValue = "Consumable/ConsumableShopPage/materialList/0";
        serialized.FindProperty("finger").objectReferenceValue = finger;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Imports the original skeleton, atlas and texture as persistent Unity assets.</summary>
    /// <returns>The hospital interior skeleton asset.</returns>
    /// <param name="spineName">Original skeleton and asset folder name.</param>
    private static SkeletonDataAsset ImportInterior(string spineName)
    {
        string spinePath = "Assets/Data/Widgets/Inn/Resources/" + spineName + "/";
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

    /// <summary>Attaches original Spine rendering to a node created by the formal UGUI tool.</summary>
    /// <param name="node">Authored graphic node.</param>
    /// <param name="data">Original skeleton data.</param>
    /// <param name="size">Available design-space display dimensions.</param>
    private static void AddSkeleton(Transform node, SkeletonDataAsset data, Vector2 size)
    {
        var graphic = node.GetComponent<SkeletonGraphic>();
        if (!graphic)
            graphic = node.gameObject.AddComponent<SkeletonGraphic>();

        graphic.skeletonDataAsset = data;
        string materialPath = "Assets/Data/Widgets/Inn/Resources/e_ui_bg_hotel_1/Interior.mat";
        graphic.material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        graphic.startingAnimation = ActorAnimationNames.idle;
        graphic.startingLoop = true;
        graphic.allowMultipleCanvasRenderers = true;
        graphic.raycastTarget = false;
        graphic.Initialize(true);
        graphic.Update(0f);
        float[] vertices = null;
        graphic.Skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertices);
        float scale = node.name == "RoomSpine" ? size.x / (width * 100f)
            : Mathf.Min(size.x / (width * 100f), size.y / (height * 100f));
        node.localScale = Vector3.one * scale;
        var rect = (RectTransform)node;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        Vector2 origin = node.name == "Patient" ? new Vector2(765f, -565f) : new Vector2(540f, -325f);
        rect.anchoredPosition = origin
            + new Vector2(-(x + width * 0.5f) * scale * 100f, -(y + height * 0.5f) * scale * 100f);
        if (node.name == "RoomSpine")
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(y + height) * scale * 100f);
        else
            rect.anchoredPosition = new Vector2(765f - (x + width * 0.5f) * scale * 100f,
                -560f - y * scale * 100f);
    }

}
