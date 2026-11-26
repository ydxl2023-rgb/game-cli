using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Authors original hotel and hospital rescue variants in the existing Home scene.</summary>
[InitializeOnLoad]
public static class MiniwowSecondRescueSetup
{
    /// <summary>Explicit editor installation request.</summary>
    private const string requestPath = "../tools/opening/second.request";

    /// <summary>Registers the idle request processor.</summary>
    static MiniwowSecondRescueSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Installs only while the editor is idle and not playing.</summary>
    private static void Poll()
    {
        if (!File.Exists(requestPath) || EditorApplication.isPlayingOrWillChangePlaymode
            || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        File.Delete(requestPath);
        try
        {
            Install();
            File.WriteAllText("../tools/opening/second-install.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText("../tools/opening/second-install.json", new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Adds the selected original models and binds native presentation references.</summary>
    private static void Install()
    {
        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = scene.GetRootGameObjects().Select(root => root.GetComponent<HomeScene>()).Single(item => item);
        var hotel = nodes.Single(item => item.name == "Hotel_60017");
        var hospital = nodes.Single(item => item.name == "Hospital_60018");
        var auction = nodes.Single(item => item.name == "AuctionHouse_60019");
        var burnedHotel = ImportModel("hotel", hotel.Find("Model"));
        var burnedHospital = ImportModel("hospital-burned", hospital.Find("Model"));
        var rescuedHospital = ImportModel("hospital-rescued", hospital.Find("Model"));
        var initialHotel = hotel.Find("Model/Hotel_initial").gameObject;
        foreach (Transform child in hospital.Find("Model"))
            child.gameObject.SetActive(child.gameObject == burnedHospital);

        initialHotel.SetActive(false);
        burnedHotel.SetActive(true);
        var owner = nodes.Single(item => item.name == "NPC_hotel").GetComponent<ActorShape>();
        owner.gameObject.SetActive(false);
        var opening = new SerializedObject(home.GetComponent<HomeOpeningScene>());
        BindArray(opening, "hotelModels", new UnityEngine.Object[] { burnedHotel, initialHotel });
        BindArray(opening, "hospitalModels", new UnityEngine.Object[] { burnedHospital, rescuedHospital });
        opening.FindProperty("hotelOwner").objectReferenceValue = owner;
        BindArray(opening, "rescueTargets", new UnityEngine.Object[] {
            auction.Find("PeopleBubble"), hotel.Find("PeopleBubble"), hospital.Find("PeopleBubble")
        });

        var template = (GameObject)opening.FindProperty("auctionFire").objectReferenceValue;
        var effects = new GameObject("SecondRescueEffects");
        var existing = home.transform.Find(effects.name);
        if (existing)
            UnityEngine.Object.DestroyImmediate(existing.gameObject);

        effects.transform.SetParent(home.transform, false);
        var hotelFire = UnityEngine.Object.Instantiate(template, effects.transform);
        hotelFire.name = "HotelFire";
        hotelFire.transform.position = hotel.Find("PeopleBubble").position - Vector3.up * 2f;
        var hospitalFire = UnityEngine.Object.Instantiate(template, effects.transform);
        hospitalFire.name = "HospitalFire";
        hospitalFire.transform.position = hospital.Find("PeopleBubble").position;
        BindArray(opening, "secondFires", new UnityEngine.Object[] { hotelFire, hospitalFire });
        opening.ApplyModifiedPropertiesWithoutUndo();

        const string portraitPath = "Assets/Data/Widgets/Common/Icons/Conversation_hotel.png";
        File.Copy("../extracted-resources/fairygui/CharacterIcon/NPC/Conversation_hotel.png", portraitPath, true);
        AssetDatabase.ImportAsset(portraitPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(portraitPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.SaveAndReimport();
        const string widgetPath = "Assets/Data/Widgets/Home/UIHomeOpening.prefab";
        var widget = PrefabUtility.LoadPrefabContents(widgetPath);
        try
        {
            var data = new SerializedObject(widget.GetComponent<UIHomeOpening>());
            data.FindProperty("hotelOwner").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(portraitPath);
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(widget, widgetPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(widget);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Imports one selected variant through the shared native importer.</summary>
    /// <param name="name">The audited export directory.</param>
    /// <param name="anchor">The original building model anchor.</param>
    /// <returns>The installed model root.</returns>
    private static GameObject ImportModel(string name, Transform anchor)
    {
        string source = "../tools/main-scene/" + name;
        string destination = "Assets/Data/Scenes/Home/Opening/" + name;
        var data = JObject.Parse(File.ReadAllText(source + "/scene.json"));
        MiniwowMainSceneSetup.ImportAssets(data, source, destination, out var meshes, out var materials);
        MiniwowAuctionSetup.InstallModel(data, anchor, meshes, materials);

        return anchor.Find((string)data["nodes"][0]["name"]).gameObject;
    }

    /// <summary>Writes a fixed ordered scene reference array.</summary>
    /// <param name="data">The serialized scene presentation.</param>
    /// <param name="name">The private field name.</param>
    /// <param name="values">The authored references.</param>
    private static void BindArray(SerializedObject data, string name, UnityEngine.Object[] values)
    {
        var array = data.FindProperty(name);
        array.arraySize = values.Length;
        for (int index = 0; index < values.Length; index++)
            array.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
    }
}
