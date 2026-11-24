using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Rebuilds original scene geometry from audited neutral data using Unity asset APIs.</summary>
[InitializeOnLoad]
public static class MiniwowMainSceneSetup
{
    /// <summary>Local evidence folder outside the Unity compilation tree.</summary>
    private const string sourceDirectory = "../tools/main-scene";

    /// <summary>Product-owned destination for the recovered scene dependencies.</summary>
    private const string assetDirectory = "Assets/Data/Scenes/Home/Recovered";

    /// <summary>Registers the explicit-request installer.</summary>
    static MiniwowMainSceneSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Processes one authorized installation while the editor is idle.</summary>
    private static void Poll()
    {
        string request = sourceDirectory + "/install.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        File.Delete(request);
        try
        {
            Build();
        }
        catch (Exception exception)
        {
            File.WriteAllText(sourceDirectory + "/install-result.json", new JObject { ["ok"] = false, ["error"] = exception.ToString() }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Creates typed meshes, materials and a scene without importing original scripts.</summary>
    private static void Build()
    {
        string identity = File.ReadAllText("ProjectSettings/GameCLIProject.json");
        if (!identity.Contains("83c29c9f-21c1-4597-9691-6707ca146fa7") || !identity.Contains("p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3"))
            throw new InvalidOperationException("Unexpected project identity.");

        JObject data = JObject.Parse(File.ReadAllText(sourceDirectory + "/scene.json"));
        Directory.CreateDirectory(assetDirectory);
        foreach (JObject texture in data["textures"])
            File.Copy(sourceDirectory + "/" + (string)texture["file"], assetDirectory + "/" + (string)texture["id"] + ".png", true);

        AssetDatabase.Refresh();
        foreach (JObject texture in data["textures"])
        {
            string path = assetDirectory + "/" + (string)texture["id"] + ".png";
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }

        Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        foreach (JObject entry in data["meshes"])
        {
            string id = (string)entry["id"];
            JObject raw = JObject.Parse(File.ReadAllText(sourceDirectory + "/" + (string)entry["file"]));
            Mesh mesh = new Mesh { name = (string)raw["name"], indexFormat = IndexFormat.UInt32 };
            mesh.vertices = Vectors3(raw["vertices"]);
            Vector3[] normals = Vectors3(raw["normals"]);
            if (normals.Length == mesh.vertexCount)
                mesh.normals = normals;

            mesh.uv = Vectors2(raw["uv"]);
            mesh.uv2 = Vectors2(raw["uv2"]);
            float[] colors = raw["colors"].ToObject<float[]>();
            if (colors.Length == mesh.vertexCount * 4)
                mesh.colors = Enumerable.Range(0, mesh.vertexCount).Select(index => new Color(colors[index * 4], colors[index * 4 + 1], colors[index * 4 + 2], colors[index * 4 + 3])).ToArray();
            else
                mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();

            JArray submeshes = (JArray)raw["submeshes"];
            mesh.subMeshCount = submeshes.Count;
            for (int index = 0; index < submeshes.Count; index++)
                mesh.SetTriangles(submeshes[index].ToObject<int[]>(), index);

            mesh.RecalculateBounds();
            if (normals.Length != mesh.vertexCount)
                mesh.RecalculateNormals();

            string path = assetDirectory + "/" + id + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing)
            {
                EditorUtility.CopySerialized(mesh, existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                mesh = existing;
            }
            else
                AssetDatabase.CreateAsset(mesh, path);

            meshes.Add(id, mesh);
        }

        Shader shader = Shader.Find("Miniwow/RecoveredScene");
        if (!shader)
            throw new InvalidOperationException("Scene presentation shader is not compiled.");

        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        foreach (JObject entry in data["materials"])
        {
            string id = (string)entry["id"];
            string path = assetDirectory + "/" + id + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = !material;
            if (create)
                material = new Material(shader);

            material.shader = shader;
            material.name = (string)entry["name"];
            string texture = (string)entry["texture"];
            material.mainTexture = string.IsNullOrEmpty(texture) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(assetDirectory + "/" + texture + ".png");
            material.mainTextureScale = Vector2Value(entry["scale"]);
            material.mainTextureOffset = Vector2Value(entry["offset"]);
            material.color = ColorValue(entry["colors"]["_Color"], Color.white);
            JObject values = (JObject)entry["floats"];
            float surface = (float?)values["_Surface"] ?? 0;
            material.SetFloat("_AlphaClip", surface == 1 ? 1 : 0);
            material.SetFloat("_Cutoff", (float?)values["_Cutoff"] ?? 0.5f);
            material.SetFloat("_Cull", (float?)values["_Cull"] ?? 2);
            material.SetFloat("_ZWrite", (float?)values["_ZWrite"] ?? 1);
            material.SetFloat("_SrcBlend", surface == 2 ? 5 : 1);
            material.SetFloat("_DstBlend", surface == 2 ? 10 : 0);
            int queue = (int)entry["queue"];
            material.renderQueue = queue >= 0 ? queue : surface == 2 ? 3000 : surface == 1 ? 2450 : 2000;
            if (create)
                AssetDatabase.CreateAsset(material, path);
            else
                EditorUtility.SetDirty(material);

            materials.Add(id, material);
        }

        string scenePath = "Assets/Scenes/Home.unity";
        Scene prior = EditorSceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool opened = !scene.isLoaded;
        if (opened)
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        EditorSceneManager.SetActiveScene(scene);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "RecoveredBuildingScene")
                UnityEngine.Object.DestroyImmediate(root);
            else if (root.name == "HomeEnvironmentCanvas")
                root.SetActive(false);
        }

        GameObject recovered = new GameObject("RecoveredBuildingScene");
        Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
        foreach (JObject node in data["nodes"])
            objects.Add((string)node["id"], new GameObject((string)node["name"]));

        foreach (JObject node in data["nodes"])
        {
            GameObject target = objects[(string)node["id"]];
            string parent = (string)node["parent"];
            target.transform.SetParent(parent == "0" ? recovered.transform : objects[parent].transform, false);
            target.transform.localPosition = Vector3Value(node["position"]);
            target.transform.localRotation = new Quaternion((float)node["rotation"]["x"], (float)node["rotation"]["y"], (float)node["rotation"]["z"], (float)node["rotation"]["w"]);
            target.transform.localScale = Vector3Value(node["scale"]);
            target.layer = 0;
            target.SetActive((bool)node["active"]);
            string meshId = (string)node["mesh"];
            string builtin = (string)node["builtinMesh"];
            if (!string.IsNullOrEmpty(meshId) || !string.IsNullOrEmpty(builtin))
            {
                MeshFilter filter = target.AddComponent<MeshFilter>();
                filter.sharedMesh = !string.IsNullOrEmpty(meshId) ? meshes[meshId] : Resources.GetBuiltinResource<Mesh>(builtin + ".fbx");
                MeshRenderer renderer = target.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = node["materials"].Select(id => materials[(string)id]).ToArray();
                renderer.enabled = (bool)node["rendererEnabled"];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            if (node["camera"] != null && (string)node["sourceGameObjectId"] == "2")
            {
                JObject settings = (JObject)node["camera"];
                Camera camera = target.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = ColorValue(settings["m_BackGroundColor"], Color.black);
                camera.fieldOfView = (float)settings["field of view"];
                camera.nearClipPlane = (float)settings["near clip plane"];
                camera.farClipPlane = (float)settings["far clip plane"];
                camera.depth = 1;
                camera.cullingMask = 1;
                camera.allowHDR = false;
            }
        }

        // BuildingPlayer startup focuses initPos + (0, 3, 0); the saved editor camera is not the runtime view.
        Camera mainCamera = recovered.GetComponentInChildren<Camera>(true);
        Vector3 focus = new Vector3(-23.90721f, 38.17353f, 14.11279f);
        Vector3 forward = mainCamera.transform.forward;
        mainCamera.transform.parent.position = focus - forward * (focus.y / forward.y);
        HomeSceneCamera controls = mainCamera.gameObject.AddComponent<HomeSceneCamera>();
        SerializedObject bindings = new SerializedObject(controls);
        bindings.FindProperty("sceneCamera").objectReferenceValue = mainCamera;
        bindings.FindProperty("cameraHandle").objectReferenceValue = mainCamera.transform.parent;
        bindings.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        if (opened)
            EditorSceneManager.CloseScene(scene, true);

        EditorSceneManager.SetActiveScene(prior);
        File.WriteAllText(sourceDirectory + "/install-result.json", new JObject { ["ok"] = true, ["nodes"] = objects.Count, ["meshes"] = meshes.Count, ["materials"] = materials.Count }.ToString());
    }

    /// <summary>Reads a source vector without changing coordinate handedness.</summary>
    private static Vector3 Vector3Value(JToken value)
    {
        return new Vector3((float)value["x"], (float)value["y"], (float)value["z"]);
    }

    /// <summary>Reads a source texture scale or offset.</summary>
    private static Vector2 Vector2Value(JToken value)
    {
        return new Vector2((float)value["x"], (float)value["y"]);
    }

    /// <summary>Reads source tint values.</summary>
    private static Color ColorValue(JToken value, Color fallback)
    {
        return value == null ? fallback : new Color((float)value["r"], (float)value["g"], (float)value["b"], (float)value["a"]);
    }

    /// <summary>Expands flattened mesh vertices or normals.</summary>
    private static Vector3[] Vectors3(JToken value)
    {
        float[] values = value.ToObject<float[]>();

        return Enumerable.Range(0, values.Length / 3).Select(index => new Vector3(values[index * 3], values[index * 3 + 1], values[index * 3 + 2])).ToArray();
    }

    /// <summary>Expands flattened texture coordinates.</summary>
    private static Vector2[] Vectors2(JToken value)
    {
        float[] values = value.ToObject<float[]>();

        return Enumerable.Range(0, values.Length / 2).Select(index => new Vector2(values[index * 2], values[index * 2 + 1])).ToArray();
    }
}
