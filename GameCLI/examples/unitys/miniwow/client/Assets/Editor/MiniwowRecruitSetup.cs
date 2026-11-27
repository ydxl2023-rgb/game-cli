using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>Authors recovered recruitment actors and formal UGUI bindings through native asset APIs.</summary>
[InitializeOnLoad]
public static class MiniwowRecruitSetup
{
    /// <summary>Explicit edit-time request.</summary>
    private const string request = "Library/OpeningQA/recruit-setup.request";

    /// <summary>Registers the explicit authoring worker.</summary>
    static MiniwowRecruitSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Processes scene or widget authoring only while the editor is idle.</summary>
    private static void OnUpdate()
    {
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
            || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string operation = File.ReadAllText(request).Trim();
        File.Delete(request);
        try
        {
            if (operation == "scene")
                InstallScene();
            else if (operation == "video")
                ImportVideo();
            else
                BindWidgets();

            AssetDatabase.SaveAssets();
            File.WriteAllText("Library/OpeningQA/recruit-setup-" + operation + ".json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText("Library/OpeningQA/recruit-setup-" + operation + ".json", exception.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Creates a compatible native imported artifact while preserving the recovered source bytes.</summary>
    private static void ImportVideo()
    {
        var importer = (VideoClipImporter)AssetImporter.GetAtPath("Assets/Data/Widgets/Recruit/heroDrawVideo.mp4");

        importer.ClearTargetSettings("Standalone");
        importer.SaveAndReimport();
    }

    /// <summary>Installs actual actors and recovered original navigation destinations.</summary>
    private static void InstallScene()
    {
        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var rows = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/RecruitHeroConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = nodes.Select(node => node.GetComponent<HomeScene>()).Single(item => item);
        var fields = new SerializedObject(home);
        var camera = (Camera)fields.FindProperty("sceneCamera").objectReferenceValue;
        var actors = fields.FindProperty("actors");
        var presentation = home.GetComponent<HomeRecruitScene>();
        if (!presentation)
            presentation = home.gameObject.AddComponent<HomeRecruitScene>();

        var shapes = new ActorShape[2];
        int index = 0;
        foreach (var property in ((JObject)rows["data"]).Properties())
        {
            var row = property.Value;
            string name = "RecruitHero" + (int)row["ID"];
            var existing = home.transform.Find(name);
            var root = existing ? existing.gameObject : new GameObject(name);
            root.transform.SetParent(home.transform, false);
            root.transform.position = new Vector3((float)row["StartX"], (float)row["StartY"], (float)row["StartZ"]);
            var visual = root.transform.Find("Visual");
            if (!visual)
            {
                visual = new GameObject("Visual").transform;
                visual.SetParent(root.transform, false);
            }

            visual.rotation = Quaternion.Euler(30f, -135f, 0f);
            visual.localScale = Vector3.one * 1.65f;
            var skeleton = MiniwowTownSetup.ImportSkeleton((string)row["Model"]);
            var animation = visual.GetComponent<SkeletonAnimation>();
            if (!animation)
                animation = SkeletonAnimation.AddToGameObject(visual.gameObject, skeleton);

            var shape = root.GetComponent<ActorShape>();
            if (!shape)
                shape = root.AddComponent<ActorShape>();

            ConfigureShape(shape, animation, camera, ActorAnimationNames.walk);
            ConfigureActor(actors, (int)row["ActorID"], (int)row["ID"], shape, root.transform);
            shapes[index++] = shape;
        }

        var brother = nodes.Single(node => node.name == "NPC_brother_2");
        var brotherData = MiniwowTownSetup.ImportSkeleton("npc_brother_2");
        var brotherAnimation = brother.GetComponent<SkeletonAnimation>();
        if (!brotherAnimation)
            brotherAnimation = SkeletonAnimation.AddToGameObject(brother.gameObject, brotherData);

        var brotherShape = brother.GetComponent<ActorShape>();
        if (!brotherShape)
            brotherShape = brother.gameObject.AddComponent<ActorShape>();

        ConfigureShape(brotherShape, brotherAnimation, camera, string.Empty);
        ConfigureActor(actors, 8, 60021, brotherShape, brother);
        var selections = fields.FindProperty("npcSelections");
        var entrances = new Transform[2];
        string[] buildings = { "Bro_60021", "Horn_60004" };
        int[] ids = { 60021, 60004 };
        for (int slot = 0; slot < buildings.Length; slot++)
        {
            var building = nodes.Single(node => node.name == buildings[slot]);
            var children = building.GetComponentsInChildren<Transform>(true);
            var entrance = children.Single(node => node.name == "Collider");
            var mount = children.Single(node => node.name == "EntranceBubble");
            var effect = children.Single(node => node.name == "TriggerEffect");
            entrances[slot] = mount;
            int rowIndex = -1;
            for (int item = 0; item < selections.arraySize; item++)
            {
                if (selections.GetArrayElementAtIndex(item).FindPropertyRelative("buildingId").intValue == ids[slot])
                    rowIndex = item;
            }

            if (rowIndex < 0)
                rowIndex = selections.arraySize++;

            var row = selections.GetArrayElementAtIndex(rowIndex);
            row.FindPropertyRelative("buildingId").intValue = ids[slot];
            row.FindPropertyRelative("entryAnchor").objectReferenceValue = mount;
            row.FindPropertyRelative("titleAnchor").objectReferenceValue = mount;
            row.FindPropertyRelative("occupant").objectReferenceValue = building.gameObject;
            row.FindPropertyRelative("entrance").objectReferenceValue = entrance;
            row.FindPropertyRelative("center").vector3Value = Vector3.zero;
            row.FindPropertyRelative("size").vector3Value = Vector3.one * 2f;
            row.FindPropertyRelative("effect").objectReferenceValue = effect.gameObject;
        }

        var presentationFields = new SerializedObject(presentation);
        presentationFields.FindProperty("sceneCamera").objectReferenceValue = camera;
        SetObjects(presentationFields.FindProperty("entrances"), entrances);
        SetObjects(presentationFields.FindProperty("recruits"), shapes);
        presentationFields.ApplyModifiedPropertiesWithoutUndo();
        fields.FindProperty("recruitment").objectReferenceValue = presentation;
        fields.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.OpenScene((string)scenes["data"]["10001"]["Path"]);
    }

    /// <summary>Configures the established actor presentation contract.</summary>
    /// <param name="shape">Actor view.</param>
    /// <param name="animation">Recovered skeleton.</param>
    /// <param name="camera">Scene projection camera.</param>
    /// <param name="walk">Original movement clip or empty for a resident.</param>
    private static void ConfigureShape(ActorShape shape, SkeletonAnimation animation, Camera camera, string walk)
    {
        var fields = new SerializedObject(shape);
        fields.FindProperty("animationView").objectReferenceValue = animation;
        fields.FindProperty("sceneCamera").objectReferenceValue = camera;
        fields.FindProperty("walkAnimation").stringValue = walk;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Upserts a logical actor binding without replacing existing actors.</summary>
    /// <param name="actors">Existing collection.</param>
    /// <param name="id">Actor instance identity.</param>
    /// <param name="configId">Original identity.</param>
    /// <param name="shape">Actor presentation.</param>
    /// <param name="origin">Original initial position.</param>
    private static void ConfigureActor(SerializedProperty actors, int id, int configId,
        ActorShape shape, Transform origin)
    {
        int selected = -1;
        for (int index = 0; index < actors.arraySize; index++)
        {
            if (actors.GetArrayElementAtIndex(index).FindPropertyRelative("actorId").intValue == id)
                selected = index;
        }

        if (selected < 0)
            selected = actors.arraySize++;

        var actor = actors.GetArrayElementAtIndex(selected);
        actor.FindPropertyRelative("actorId").intValue = id;
        actor.FindPropertyRelative("configId").intValue = configId;
        actor.FindPropertyRelative("destinationId").intValue = 0;
        actor.FindPropertyRelative("moveSpeed").floatValue = 3.2f;
        actor.FindPropertyRelative("shape").objectReferenceValue = shape;
        SetObjects(actor.FindPropertyRelative("route"), new[] { origin });
    }

    /// <summary>Assigns explicit serialized object arrays.</summary>
    /// <param name="property">Array field.</param>
    /// <param name="values">Native asset or scene references.</param>
    private static void SetObjects(SerializedProperty property, UnityEngine.Object[] values)
    {
        property.arraySize = values.Length;
        for (int index = 0; index < values.Length; index++)
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
    }

    /// <summary>Binds native movie and world projections after formal UGUI and Metadata generation.</summary>
    private static void BindWidgets()
    {
        ImportVideo();
        foreach (var row in JArray.Parse(File.ReadAllText("../tools/recruit/slice-borders.json")))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath((string)row["path"]);
            var border = row["border"];
            importer.spriteBorder = new Vector4((float)border[0], (float)border[1],
                (float)border[2], (float)border[3]);
            importer.SaveAndReimport();
        }

        foreach (string portrait in Directory.GetFiles("Assets/Data/Widgets/Recruit/Portraits", "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(portrait);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        string path = "Assets/Data/Widgets/Recruit/UIRecruit.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var widget = root.GetComponent<UIRecruit>();
            var movie = root.GetComponent<VideoPlayer>();
            if (!movie)
                movie = root.AddComponent<VideoPlayer>();

            movie.playOnAwake = false;
            movie.isLooping = false;
            movie.renderMode = VideoRenderMode.APIOnly;
            movie.audioOutputMode = VideoAudioOutputMode.None;
            movie.timeUpdateMode = VideoTimeUpdateMode.GameTime;
            movie.clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Data/Widgets/Recruit/heroDrawVideo.mp4");
            var screen = root.GetComponentsInChildren<Transform>(true).Single(node => node.name == "MovieScreen");
            var image = screen.GetComponent<Image>();
            if (image)
                UnityEngine.Object.DestroyImmediate(image);

            var raw = screen.GetComponent<RawImage>();
            if (!raw)
                raw = screen.gameObject.AddComponent<RawImage>();

            var fields = new SerializedObject(widget);
            fields.FindProperty("movie").objectReferenceValue = movie;
            fields.FindProperty("screen").objectReferenceValue = raw;
            fields.ApplyModifiedPropertiesWithoutUndo();
            ConfigureGraphics(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        path = "Assets/Data/Widgets/Home/UIHomeRecruitEntry.prefab";
        root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            string[] names = { "BrotherClick", "HornClick", "FirstClick", "SecondClick" };
            int[] ids = { 60106, 60112, 60113, 60114 };
            var widget = root.GetComponent<UIHomeRecruitEntry>();
            var fields = new SerializedObject(widget);
            var buttons = names.Select(name => root.GetComponentsInChildren<RectTransform>(true)
                .Single(node => node.name == name)).ToArray();
            SetObjects(fields.FindProperty("bubbles"), buttons);
            var steps = fields.FindProperty("steps");
            steps.arraySize = ids.Length;
            for (int index = 0; index < ids.Length; index++)
            {
                steps.GetArrayElementAtIndex(index).intValue = ids[index];
                buttons[index].anchorMin = Vector2.one * 0.5f;
                buttons[index].anchorMax = Vector2.one * 0.5f;
                buttons[index].pivot = new Vector2(0.5f, 0f);
            }

            fields.ApplyModifiedPropertiesWithoutUndo();
            ConfigureGraphics(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        path = "Assets/Data/Widgets/BattleEntry/UIBattleEntry.prefab";
        root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            ConfigureGraphics(root);
            BindGuide(root, "StageClick", "Level/ExploreMapPage/BossLayer/101001/Button");
            BindGuide(root, "SelectClick", "Level/BossListPage/List/0");
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Leaves raycasts only on actual controls and modal surfaces.</summary>
    /// <param name="root">Authored widget.</param>
    private static void ConfigureGraphics(GameObject root)
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Data/Widgets/Common/Fonts/hanyijingkai.ttf");
        foreach (var text in root.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            if (text.name == "Dialogue" || text.name == "Description" || text.name == "StageName")
            {
                text.color = new Color32(62, 44, 30, 255);
                text.alignment = text.name == "StageName" ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft;
            }
        }

        foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = graphic.GetComponent<Button>() || graphic is RawImage
                || graphic.name == "MapBackground" || graphic.name == "Panel";
            if (graphic is Image image)
            {
                if (image.sprite && image.sprite.border != Vector4.zero)
                {
                    image.type = Image.Type.Sliced;
                    image.pixelsPerUnitMultiplier = 2f / 3f;
                }

                image.color = image.sprite || image.GetComponent<Button>()
                    ? Color.white : new Color(1f, 1f, 1f, 0f);
                if (image.GetComponent<Button>() && !image.sprite)
                    image.color = new Color(1f, 1f, 1f, 0f);

                if (image.name == "Portrait")
                    image.color = Color.white;
            }
        }
    }

    /// <summary>Registers existing original guide paths on actual interactive controls.</summary>
    /// <param name="root">Page root.</param>
    /// <param name="name">Authored button name.</param>
    /// <param name="key">Original configured guide target.</param>
    private static void BindGuide(GameObject root, string name, string key)
    {
        var button = root.GetComponentsInChildren<Button>(true).Single(item => item.name == name);
        var marker = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Task/UITaskGuideFinger.prefab");
        var existing = button.transform.Find(marker.name);
        var finger = existing ? existing.gameObject :
            (GameObject)PrefabUtility.InstantiatePrefab(marker, button.transform);
        var rect = (RectTransform)finger.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        foreach (var graphic in finger.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

        finger.SetActive(false);
        var target = button.GetComponent<TaskGuideTarget>();
        if (!target)
            target = button.gameObject.AddComponent<TaskGuideTarget>();

        var fields = new SerializedObject(target);
        fields.FindProperty("targetKey").stringValue = key;
        fields.FindProperty("finger").objectReferenceValue = finger;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }
}
