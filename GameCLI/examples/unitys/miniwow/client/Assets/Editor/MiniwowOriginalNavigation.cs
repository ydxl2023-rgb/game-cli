using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>Restores original navigation tiles through Unity serialization without rebaking geometry.</summary>
public static class MiniwowOriginalNavigation
{
    /// <summary>Validates the retained tile payload and saves a native navigation asset.</summary>
    /// <returns>The original navigation asset with its recovered agent type.</returns>
    public static NavMeshData Import()
    {
        string json = File.ReadAllText("../extracted-resources/assets/data/scenes/building_sdc/building_sdc/navmesh-mesh.asset.json");
        JObject source = JObject.Parse(json);
        const string path = "Assets/Data/Scenes/Town/OriginalTownNavigation.asset";
        NavMeshData data = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if (!data)
        {
            data = new NavMeshData((int)source["m_AgentTypeID"]);
            AssetDatabase.CreateAsset(data, path);
        }

        source["serializedVersion"] = 2;
        source["m_NavMeshBuildSettings"]["serializedVersion"] = 3;
        var wrapped = new JObject { ["NavMeshData"] = source };
        EditorJsonUtility.FromJsonOverwrite(wrapped.ToString(), data);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        JObject restored = (JObject)JObject.Parse(EditorJsonUtility.ToJson(data))["NavMeshData"];
        File.WriteAllText("Library/OriginalNavigationSerialized.json", restored.ToString());
        JArray sourceTiles = (JArray)source["m_NavMeshTiles"];
        JArray targetTiles = (JArray)restored["m_NavMeshTiles"];
        if (sourceTiles.Count != targetTiles.Count)
            throw new InvalidOperationException("Original navigation tile count changed.");

        for (int index = 0; index < sourceTiles.Count; index++)
        {
            string expectedHash = string.Concat(Enumerable.Range(0, 16).Select(offset => ((int)sourceTiles[index]["m_Hash"]["bytes[" + offset + "]"]).ToString("x2")));
            if (!JToken.DeepEquals(sourceTiles[index]["m_MeshData"], targetTiles[index]["m_MeshData"]) || expectedHash != (string)targetTiles[index]["m_Hash"]["Hash"])
                throw new InvalidOperationException("Original navigation tile bytes or hash changed.");
        }

        InstallAgentType((int)source["m_AgentTypeID"]);

        NavMeshDataInstance registration = NavMesh.AddNavMeshData(data);
        try
        {
            var filter = new NavMeshQueryFilter {
                agentTypeID = (int)source["m_AgentTypeID"],
                areaMask = NavMesh.AllAreas
            };
            var origin = new Vector3(-23.90721f, 35.17353f, 14.11279f);
            bool sampled = NavMesh.SamplePosition(origin, out NavMeshHit hit, 2f, filter);
            File.WriteAllText("../tools/player/original-navigation-import.json", JsonConvert.SerializeObject(new {
                agentType = (int)source["m_AgentTypeID"],
                tileCount = ((JArray)source["m_NavMeshTiles"]).Count,
                exactTiles = true,
                vertices = NavMesh.CalculateTriangulation().vertices.Length,
                sampled,
                position = new[] { hit.position.x, hit.position.y, hit.position.z },
                settings = restored["m_NavMeshBuildSettings"]
            }, Formatting.Indented));
            if (!sampled)
                throw new InvalidOperationException("Original player spawn cannot resolve on the imported navigation.");
        }
        finally
        {
            registration.Remove();
        }

        return data;
    }
    /// <summary>Adds the original agent type while preserving unrelated project settings.</summary>
    /// <param name="agentType">Recovered player navigation type.</param>
    private static void InstallAgentType(int agentType)
    {
        const string settingsPath = "ProjectSettings/NavMeshAreas.asset";
        UnityEngine.Object settings = AssetDatabase.LoadAllAssetsAtPath(settingsPath).Single();
        JObject wrapper = JObject.Parse(EditorJsonUtility.ToJson(settings));
        JObject current = (JObject)wrapper.Properties().First().Value;
        JArray types = (JArray)current["m_Settings"];
        if (types.Any(item => (int)item["agentTypeID"] == agentType))
            return;

        JObject original = JObject.Parse(File.ReadAllText("../extracted-resources/_unmapped/285cf4bb138d785f.webgl.data.unityweb/NavMeshProjectSettings/14__971955710bf6.json"));
        types.Add(original["m_Settings"].Single(item => (int)item["agentTypeID"] == agentType).DeepClone());
        ((JArray)current["m_SettingNames"]).Add("Hero");
        EditorJsonUtility.FromJsonOverwrite(wrapper.ToString(), settings);
        EditorUtility.SetDirty(settings);
        UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { settings }, settingsPath, true);
    }

}
