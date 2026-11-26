using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>Checks original navigation fidelity and the boundary-sliding regression.</summary>
public static class MiniwowOriginalPlayerProbe
{
    /// <summary>Finds original walkable locations falsely blocked by the previous rebake.</summary>
    /// <returns>Recorded coverage differences within the visible town district.</returns>
    public static string CompareCoverage()
    {
        var config = Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER);
        var original = new NavMeshQueryFilter { agentTypeID = config.AgentTypeId, areaMask = NavMesh.AllAreas };
        var rebaked = new NavMeshQueryFilter { agentTypeID = 0, areaMask = NavMesh.AllAreas };
        NavMeshData previous = AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/Data/Scenes/Town/TownNavigation.asset");
        NavMeshDataInstance registration = NavMesh.AddNavMeshData(previous);
        var differences = new List<float[]>();
        int originalCount = 0;
        int rebakedCount = 0;
        try
        {
            for (int x = -50; x <= 10; x += 2)
            {
                for (int z = -30; z <= 36; z += 2)
                {
                    var point = new Vector3(x, config.SpawnY, z);
                    bool source = NavMesh.SamplePosition(point, out NavMeshHit sourceHit, 0.2f, original);
                    bool old = NavMesh.SamplePosition(point, out NavMeshHit oldHit, 0.2f, rebaked);
                    if (source)
                        originalCount++;

                    if (old)
                        rebakedCount++;

                    if (source && !old)
                        differences.Add(new[] { sourceHit.position.x, sourceHit.position.y, sourceHit.position.z });
                }
            }
        }
        finally
        {
            registration.Remove();
        }

        string result = JsonConvert.SerializeObject(new { originalCount, rebakedCount, previouslyBlocked = differences }, Formatting.Indented);
        File.WriteAllText("../tools/player/navigation-coverage.json", result);

        return result;
    }

    /// <summary>Exercises oblique movement at actual original navigation boundaries.</summary>
    /// <returns>Evidence that native movement slides beyond a ray-only stop.</returns>
    public static string VerifyWallSlide()
    {
        var config = Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER);
        var filter = new NavMeshQueryFilter { agentTypeID = config.AgentTypeId, areaMask = NavMesh.AllAreas };
        NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
        var root = new GameObject("OriginalWallSlideProbe");
        root.SetActive(false);
        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        agent.agentTypeID = config.AgentTypeId;
        agent.radius = config.AgentRadius;
        agent.height = config.AgentHeight;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        root.transform.position = new Vector3(config.SpawnX, config.SpawnY, config.SpawnZ);
        root.SetActive(true);
        var adapter = new HomeNavigation(agent);
        var examples = new List<object>();
        try
        {
            for (int index = 0; index < mesh.indices.Length && examples.Count < 12; index += 3)
            {
                Vector3 center = (mesh.vertices[mesh.indices[index]] + mesh.vertices[mesh.indices[index + 1]] + mesh.vertices[mesh.indices[index + 2]]) / 3f;
                if (Mathf.Abs(center.y - config.SpawnY) > 2f || !NavMesh.FindClosestEdge(center, out NavMeshHit edge, filter))
                    continue;

                Vector3 inward = Vector3.ProjectOnPlane(center - edge.position, Vector3.up).normalized;
                if (inward.sqrMagnitude < 0.5f)
                    continue;

                Vector3 candidate = edge.position + inward * 0.25f;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit start, 0.1f, filter))
                    continue;

                Vector3 tangent = Vector3.Cross(Vector3.up, inward);
                Vector3 movement = (-inward + tangent).normalized;
                Vector3 target = start.position + movement;
                if (!NavMesh.Raycast(start.position, target, out NavMeshHit rayStop, filter) || !agent.Warp(start.position))
                    continue;

                if (!adapter.TryMove(start.position, target, out Vector3 result))
                    throw new InvalidOperationException("Original agent refused a bound wall-slide request.");

                float slide = Vector3.Dot(result - rayStop.position, tangent);
                if (slide > 0.1f && NavMesh.SamplePosition(result, out NavMeshHit landed, 0.05f, filter))
                {
                    examples.Add(new {
                        start = new[] { start.position.x, start.position.y, start.position.z },
                        rayStop = new[] { rayStop.position.x, rayStop.position.y, rayStop.position.z },
                        nativeResult = new[] { result.x, result.y, result.z },
                        slide
                    });
                }
            }

            if (examples.Count < 3)
                throw new InvalidOperationException("Insufficient original wall-slide coverage.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        string evidence = JsonConvert.SerializeObject(new { passed = true, examples }, Formatting.Indented);
        File.WriteAllText("../tools/player/wall-slide-evidence.json", evidence);

        return evidence;
    }
}
