using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Original room animation names.</summary>
public static class HospitalRoomAnimations
{
    /// <summary>Foreground counter animation from the original room prefab.</summary>
    public const string counter = "idle2";
}

/// <summary>Binds native references after formal hospital UGUI generation.</summary>
[InitializeOnLoad]
public static class MiniwowHospitalSetup
{
    /// <summary>Explicit authoring request directory.</summary>
    private const string toolsPath = "../tools/hospital-ui/";

    /// <summary>Hospital-specific Spine asset directory.</summary>
    private const string spinePath = "Assets/Data/Widgets/Hospital/Resources/Spine/";

    /// <summary>Original hospital interior skeleton name.</summary>
    private const string spineName = "e_ui_bg_priest_1";

    /// <summary>Registers the explicit edit-mode installation request.</summary>
    static MiniwowHospitalSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Handles only a requested installation while the editor is idle.</summary>
    private static void Poll()
    {
        string request = toolsPath + "bind.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
            || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        bool pageOnly = File.ReadAllText(request).Trim() == "page";
        File.Delete(request);
        try
        {
            ImportBorders();
            var skeleton = ImportInterior();
            BindPage(skeleton);
            BindEntry();
            BindScene();
            AssetDatabase.SaveAssets();
            File.WriteAllText(toolsPath + "bind-result.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText(toolsPath + "bind-result.json", new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Restores original FairyGUI nine-slice borders on the selected background assets.</summary>
    private static void ImportBorders()
    {
        var rows = JArray.Parse(File.ReadAllText(toolsPath + "slice-borders.json"));
        foreach (JObject row in rows)
        {
            string path = "Assets/Data/Widgets/Hospital/Resources/" + (string)row["name"] + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var border = row["border"];
            importer.spriteBorder = new Vector4((float)border[0], (float)border[1],
                (float)border[2], (float)border[3]);
            importer.SaveAndReimport();
        }
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

    /// <summary>Binds the generated page to its authored controls and original animated interior.</summary>
    /// <param name="skeleton">Imported original interior.</param>
    private static void BindPage(SkeletonDataAsset skeleton)
    {
        const string path = "Assets/Data/Widgets/Hospital/UIHomeHospital.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var nodes = root.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            ConfigureGraphics(root);
            var binding = new SerializedObject(root.GetComponent<UIHomeHospital>());
            binding.FindProperty("materialButton").objectReferenceValue = nodes["CraftClick"].GetComponent<Button>();
            binding.FindProperty("flyingMaterial").objectReferenceValue = nodes["FlyingMaterial"];
            binding.FindProperty("productionTarget").objectReferenceValue = nodes["ProductionTarget"];
            binding.FindProperty("patient").objectReferenceValue = nodes["Patient"].gameObject;
            binding.ApplyModifiedPropertiesWithoutUndo();
            if (!nodes["Room"].GetComponent<RectMask2D>())
                nodes["Room"].gameObject.AddComponent<RectMask2D>();

            AddSkeleton(nodes["RoomSpine"], skeleton, new Vector2(1080f, 650f));
            var room = (RectTransform)nodes["RoomSpine"];
            room.localScale *= 1.2f;
            var center = new Vector2(540f, -325f);
            room.anchoredPosition = center + (room.anchoredPosition - center) * 1.2f + Vector2.down * 50f;

            AddSkeleton(nodes["CounterSpine"], skeleton, new Vector2(1080f, 650f));
            var counter = nodes["CounterSpine"].GetComponent<SkeletonGraphic>();
            counter.startingAnimation = HospitalRoomAnimations.counter;
            counter.AnimationState.SetAnimation(0, HospitalRoomAnimations.counter, true);
            nodes["CounterSpine"].localScale = nodes["RoomSpine"].localScale;
            ((RectTransform)nodes["CounterSpine"]).anchoredPosition =
                ((RectTransform)nodes["RoomSpine"]).anchoredPosition;
            AddSkeleton(nodes["Priest"], ImportPriest(), new Vector2(200f, 220f));
            var priest = (RectTransform)nodes["Priest"];
            float roomScale = nodes["RoomSpine"].localScale.x;
            priest.localScale = Vector3.one * roomScale * (0.9f / 0.641f);
            priest.anchoredPosition = ((RectTransform)nodes["RoomSpine"]).anchoredPosition
                + new Vector2(-0.056f, 0.559f) * (roomScale * 10000f / 0.641f);
            nodes["UpgradeClick"].GetComponent<Button>().interactable = false;
            nodes["AutoCraftClick"].GetComponent<Button>().interactable = false;
            nodes["AutoCraftClick"].GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);
            nodes["CapacityIcon"].GetComponent<Image>().enabled = false;
            nodes["CraftClick"].GetComponent<Image>().color = Color.clear;
            nodes["Title"].GetComponent<Text>().color = new Color(1f, 0.83f, 0.15f);
            nodes["Gem"].GetComponent<Text>().color = Color.white;
            foreach (string name in new[] { "PriceBonus", "TimeBonus", "CapacityBonus" })
                nodes[name].GetComponent<Text>().color = new Color(0.18f, 0.4f, 0.12f);

            var hero = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
                "Assets/Data/Scenes/Town/Spine/hero_20025/Skeleton.asset");
            AddSkeleton(nodes["Patient"], hero, new Vector2(110f, 125f));
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Imports the separate original priest referenced by the room prefab.</summary>
    /// <returns>The persistent priest skeleton.</returns>
    private static SkeletonDataAsset ImportPriest()
    {
        const string path = "Assets/Data/Widgets/Hospital/Resources/Priest/";
        var texture = (TextureImporter)AssetImporter.GetAtPath(path + "npc_priest.png");
        texture.textureType = TextureImporterType.Default;
        texture.mipmapEnabled = false;
        texture.textureCompression = TextureImporterCompression.Uncompressed;
        texture.SaveAndReimport();
        var material = AssetDatabase.LoadAssetAtPath<Material>(path + "Priest.mat");
        if (!material)
        {
            material = new Material(Shader.Find("Spine/SkeletonGraphic"));
            AssetDatabase.CreateAsset(material, path + "Priest.mat");
        }

        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "npc_priest.png");
        EditorUtility.SetDirty(material);
        var atlas = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(path + "Atlas.asset");
        if (!atlas)
        {
            atlas = SpineAtlasAsset.CreateRuntimeInstance(
                AssetDatabase.LoadAssetAtPath<TextAsset>(path + "npc_priest.atlas.txt"), new[] { material }, true);
            AssetDatabase.CreateAsset(atlas, path + "Atlas.asset");
        }

        var skeleton = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path + "Skeleton.asset");
        if (!skeleton)
        {
            skeleton = SkeletonDataAsset.CreateRuntimeInstance(
                AssetDatabase.LoadAssetAtPath<TextAsset>(path + "npc_priest_2.skel.bytes"), atlas, true);
            skeleton.scale = 1f;
            AssetDatabase.CreateAsset(skeleton, path + "Skeleton.asset");
        }

        return skeleton;
    }

    /// <summary>Binds the world entry controls without changing its full-stretch root.</summary>
    private static void BindEntry()
    {
        const string path = "Assets/Data/Widgets/Home/UIHomeHospitalEntry.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var nodes = root.GetComponentsInChildren<Transform>(true).ToDictionary(item => item.name);
            ConfigureGraphics(root);
            var bubble = (RectTransform)nodes["EntryVisible"];
            var title = nodes["BuildingName"].GetComponent<Text>();
            ConfigureTitle(title);
            bubble.anchorMin = new Vector2(0.5f, 0.5f);
            bubble.anchorMax = bubble.anchorMin;
            var coin = (RectTransform)nodes["Coin"];
            coin.anchorMin = bubble.anchorMin;
            coin.anchorMax = bubble.anchorMax;
            var binding = new SerializedObject(root.GetComponent<UIHomeHospitalEntry>());
            binding.FindProperty("bubble").objectReferenceValue = bubble;
            binding.FindProperty("buildingTitle").objectReferenceValue = title.rectTransform;
            foreach (string name in new[] { "MerchantEntry", "ForgeEntry", "HotelEntry" })
            {
                var entry = (RectTransform)nodes[name];
                entry.anchorMin = new Vector2(0.5f, 0.5f);
                entry.anchorMax = entry.anchorMin;
                entry.pivot = new Vector2(0.5f, 0f);
                string field = char.ToLowerInvariant(name[0]) + name.Substring(1);
                binding.FindProperty(field).objectReferenceValue = entry;
            }
            var residents = JArray.Parse(File.ReadAllText(toolsPath + "npc-entrances.json"));
            var titles = binding.FindProperty("residentTitles");
            titles.arraySize = residents.Count;
            for (int index = 0; index < residents.Count; index++)
            {
                var residentTitle = nodes[(string)residents[index]["title"]].GetComponent<Text>();
                ConfigureTitle(residentTitle);
                titles.GetArrayElementAtIndex(index).objectReferenceValue = residentTitle.rectTransform;
            }

            binding.FindProperty("coin").objectReferenceValue = coin;
            binding.FindProperty("entryButton").objectReferenceValue = nodes["RepairClick"].GetComponent<Button>();
            binding.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Applies the shared original NPC title style and bottom-center pivot.</summary>
    /// <param name="title">Authored text loaded from the language table.</param>
    private static void ConfigureTitle(Text title)
    {
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = title.rectTransform.anchorMin;
        title.rectTransform.pivot = new Vector2(0.5f, 0f);
        title.color = new Color32(75, 213, 45, 255);
        var outline = title.GetComponent<Outline>();
        if (!outline)
            outline = title.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color32(34, 49, 22, 255);
        outline.effectDistance = new Vector2(1f, -1f);
    }

    /// <summary>Restores the original hospital collider and reference bindings.</summary>
    private static void BindScene()
    {
        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = nodes.Select(node => node.GetComponent<HomeScene>()).Single(item => item);
        var hospital = nodes.Single(node => node.name == "Hospital_60018");
        var target = hospital.Find("Collider");
        var collider = target.GetComponent<BoxCollider>();
        if (!collider)
            collider = target.gameObject.AddComponent<BoxCollider>();

        var original = JObject.Parse(File.ReadAllText(
            "../research/resource-catalog/exports/BoxCollider/d3122182dd0c8e9ab4ad_2180.json"));
        collider.size = Vector(original["m_Size"]);
        collider.center = Vector(original["m_Center"]);
        collider.isTrigger = true;
        var binding = new SerializedObject(home);
        binding.FindProperty("hospitalAnchor").objectReferenceValue = hospital.Find("EntranceBubble");
        binding.FindProperty("hospitalTitleAnchor").objectReferenceValue = hospital.Find("Bubble");
        binding.FindProperty("hospitalCollider").objectReferenceValue = collider;
        binding.FindProperty("hospitalSelection").objectReferenceValue = InstallSelection(hospital);
        var residents = JArray.Parse(File.ReadAllText(toolsPath + "npc-entrances.json"));
        var selections = binding.FindProperty("npcSelections");
        selections.arraySize = residents.Count;
        for (int index = 0; index < residents.Count; index++)
        {
            var row = residents[index];
            var building = nodes.Single(node => node.name == (string)row["building"]);
            var selection = selections.GetArrayElementAtIndex(index);
            selection.FindPropertyRelative("buildingId").intValue = int.Parse(building.name.Split('_').Last());
            selection.FindPropertyRelative("entryAnchor").objectReferenceValue = building.Find("EntranceBubble");
            selection.FindPropertyRelative("occupant").objectReferenceValue = nodes
                .Single(node => node.name == (string)row["npc"]).gameObject;
            selection.FindPropertyRelative("entrance").objectReferenceValue = building.Find("Collider");
            selection.FindPropertyRelative("titleAnchor").objectReferenceValue = building.Find("Bubble");
            selection.FindPropertyRelative("center").vector3Value = Vector(row["center"]);
            selection.FindPropertyRelative("size").vector3Value = Vector(row["size"]);
            selection.FindPropertyRelative("effect").objectReferenceValue = InstallSelection(building);
        }

        binding.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>Restores the original ground particle at the original hospital effect anchor.</summary>
    /// <param name="hospital">Recovered building root.</param>
    /// <returns>The owned selection object.</returns>
    private static GameObject InstallSelection(Transform hospital)
    {
        const string folder = "Assets/Data/Scenes/Home/Opening/";
        const string source = "../extracted-resources/assets/data/effects/prefabs/_subassets/ParticleSystem/"
            + "8209274776750613049__dc4fcf016c25.json";
        var data = JObject.Parse(File.ReadAllText(source));
        var initial = data["InitialModule"];
        var anchor = hospital.Find("TriggerEffect");
        var effect = anchor.Find("effect");
        anchor.gameObject.SetActive(true);
        effect.gameObject.SetActive(false);
        var particles = effect.GetComponent<ParticleSystem>();
        if (!particles)
            particles = effect.gameObject.AddComponent<ParticleSystem>();

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.duration = (float)data["lengthInSec"];
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = (float)initial["startLifetime"]["scalar"];
        main.startSpeed = (float)initial["startSpeed"]["scalar"];
        main.startSize = (float)initial["startSize"]["scalar"];
        main.startRotation3D = true;
        main.startRotationX = (float)initial["startRotationX"]["scalar"];
        main.startRotationY = (float)initial["startRotationY"]["scalar"];
        var shape = particles.shape;
        shape.enabled = false;
        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var size = particles.sizeOverLifetime;
        size.enabled = true;
        var keys = data["SizeModule"]["curve"]["maxCurve"]["m_Curve"]
            .Select(row => new Keyframe((float)row["time"], (float)row["value"],
                (float)row["inSlope"], (float)row["outSlope"])).ToArray();
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(keys));
        var gradientData = data["ColorModule"]["gradient"]["maxGradient"];
        var alpha = Enumerable.Range(0, (int)gradientData["m_NumAlphaKeys"])
            .Select(index => new GradientAlphaKey((float)gradientData["key" + index]["a"],
                (float)gradientData["atime" + index] / 65535f)).ToArray();
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f) }, alpha);
        var color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = gradient;
        var material = AssetDatabase.LoadAssetAtPath<Material>(folder + "HospitalSelection.mat");
        if (!material)
        {
            material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, folder + "HospitalSelection.mat");
        }

        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "e_npc_xuanze_1.png");
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.alignment = ParticleSystemRenderSpace.World;
        renderer.sharedMaterial = material;

        return effect.gameObject;
    }

    /// <summary>Applies the shared font and appropriate graphic raycasts.</summary>
    /// <param name="root">Generated prefab root.</param>
    private static void ConfigureGraphics(GameObject root)
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Fonts/fangzhenglanting.ttf");
        foreach (Text text in root.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
            text.raycastTarget = false;
        }

        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            image.raycastTarget = image.GetComponent<Button>() != null || image.name == "PageBackground";
            if (image.sprite && image.sprite.border != Vector4.zero)
                image.type = Image.Type.Sliced;
        }
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
        graphic.material = AssetDatabase.LoadAssetAtPath<Material>(spinePath + "Interior.mat");
        graphic.startingAnimation = ActorAnimationNames.idle;
        graphic.startingLoop = true;
        graphic.allowMultipleCanvasRenderers = true;
        graphic.raycastTarget = false;
        graphic.Initialize(true);
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

    /// <summary>Reads the original serialized vector without changing coordinate conventions.</summary>
    /// <param name="token">Original vector object.</param>
    /// <returns>The matching Unity vector.</returns>
    private static Vector3 Vector(JToken token)
    {
        return new Vector3((float)token["x"], (float)token["y"], (float)token["z"]);
    }
}
