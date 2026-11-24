using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Creates ParticleSystem prefabs through the UnityCLI bridge.
    /// </summary>
    internal static class UnityCliParticleService
    {
        public static ParticleData Execute(UnityCliBridgeRequest request)
        {
            if (request == null) throw new InvalidOperationException("Empty request.");

            var action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            if (action == "remove-instance")
            {
                return RemoveInstance(request);
            }
            if (action != "create")
            {
                throw new InvalidOperationException("particle requires action create.");
            }

            if ((request.particleName ?? string.Empty).TrimStart().StartsWith("-", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Particle names cannot be command options.");
            }
            var particleName = SanitizeObjectName(request.particleName);
            if (string.IsNullOrEmpty(particleName))
            {
                throw new InvalidOperationException("particle create requires particleName.");
            }

            var prefabPath = NormalizeAssetPath(request.prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
            {
                prefabPath = "Assets/Resources/Effects/" + particleName + ".prefab";
            }

            RequireAssetPath(prefabPath, "--prefab");
            if (!prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("--prefab must end with .prefab.");
            }

            var texturePath = NormalizeAssetPath(request.particleTexturePath);
            var materialPath = NormalizeAssetPath(request.particleMaterialPath);
            if (!string.IsNullOrEmpty(texturePath)) RequireExistingAsset(texturePath, "--texture");
            if (!string.IsNullOrEmpty(materialPath)) RequireExistingAsset(materialPath, "--material");

            var settings = BuildSettings(request);
            var issues = new List<string>();
            if (settings.emissionRate <= 0f) issues.Add("emission-rate is zero; the effect will not emit visible particles.");
            if (!string.IsNullOrEmpty(texturePath) && AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath) == null)
            {
                issues.Add("texture is not a Texture2D asset: " + texturePath);
            }

            var data = BuildData(request, particleName, prefabPath, materialPath, texturePath, settings, issues);
            if (request.dryRun)
            {
                data.message = issues.Count == 0 ? "particle dry-run passed" : "particle dry-run completed with issues";
                return data;
            }

            EnsureParentFolder(prefabPath);
            var root = new GameObject(particleName);
            try
            {
                ConfigureParticleObject(root, settings);
                var renderer = root.GetComponent<ParticleSystemRenderer>();
                renderer.material = ResolveMaterial(particleName, prefabPath, materialPath, texturePath, out var resolvedMaterial, out var materialCreated);
                data.materialPath = resolvedMaterial;
                data.materialCreated = materialCreated;

                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException("PrefabUtility.SaveAsPrefabAsset failed: " + prefabPath);
                }

                data.prefabCreated = true;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            if (request.replaceSceneInstance)
            {
                data.sceneInstanceReplaced = ReplaceSceneInstance(particleName, prefabPath, request.objectPath, settings.position);
            }

            AssetDatabase.SaveAssets();
            data.message = "particle prefab created";
            return data;
        }

        static ParticleSettings BuildSettings(UnityCliBridgeRequest request)
        {
            var settings = new ParticleSettings
            {
                shape = string.IsNullOrWhiteSpace(request.particleShape) ? "cone" : request.particleShape.Trim().ToLowerInvariant(),
                startColor = ParseColor(request.particleStartColor, Color.white),
                duration = request.particleDurationSet ? Mathf.Max(0.05f, request.particleDuration) : 5f,
                loop = request.particleLoopSet ? request.particleLoop : true,
                startLifetime = request.particleStartLifetimeSet ? Mathf.Max(0.05f, request.particleStartLifetime) : 5f,
                startSpeed = request.particleStartSpeedSet ? request.particleStartSpeed : 1f,
                startSize = request.particleStartSizeSet ? Mathf.Max(0.01f, request.particleStartSize) : 1f,
                emissionRate = request.particleEmissionRateSet ? Mathf.Max(0f, request.particleEmissionRate) : 10f,
                maxParticles = request.particleMaxParticlesSet ? Mathf.Max(1, request.particleMaxParticles) : 1000,
                position = ParseVector3(request.particlePosition, Vector3.zero, "--position"),
                shapeScale = ParseVector3(request.particleShapeScale, Vector3.one, "--shape-scale"),
                shapeRadius = request.particleShapeRadiusSet ? Mathf.Max(0f, request.particleShapeRadius) : 1f,
                shapeAngle = request.particleShapeAngleSet ? Mathf.Clamp(request.particleShapeAngle, 0f, 90f) : 25f,
                sortingOrder = request.particleSortingOrderSet ? request.particleSortingOrder : 0,
                fadeOut = request.particleFadeOut,
                fadeStart = request.particleFadeStartSet ? Mathf.Clamp01(request.particleFadeStart) : 0.75f
            };

            settings.velocityX = ParseMinMax(request.particleVelocityX, "--velocity-x", out settings.velocityXSet);
            settings.velocityY = ParseMinMax(request.particleVelocityY, "--velocity-y", out settings.velocityYSet);
            settings.velocityZ = ParseMinMax(request.particleVelocityZ, "--velocity-z", out settings.velocityZSet);
            settings.noiseEnabled = request.particleNoiseStrengthSet || request.particleNoiseFrequencySet || request.particleNoiseScrollSpeedSet;
            settings.noiseStrength = request.particleNoiseStrengthSet ? request.particleNoiseStrength : 0f;
            settings.noiseFrequency = request.particleNoiseFrequencySet ? request.particleNoiseFrequency : 0.5f;
            settings.noiseScrollSpeed = request.particleNoiseScrollSpeedSet ? request.particleNoiseScrollSpeed : 0f;
            settings.rotationZ = ParseMinMax(request.particleRotationZ, "--rotation-z", out settings.rotationZSet);
            settings.sizeOverLifetime = ParseMinMax(request.particleSizeOverLifetime, "--size-over-lifetime", out settings.sizeOverLifetimeSet);
            return settings;
        }

        static void ConfigureParticleObject(GameObject root, ParticleSettings settings)
        {
            var particleSystem = root.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.duration = settings.duration;
            main.loop = settings.loop;
            main.startLifetime = settings.startLifetime;
            main.startSpeed = settings.startSpeed;
            main.startSize = settings.startSize;
            main.startColor = settings.startColor;
            main.maxParticles = settings.maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = particleSystem.emission;
            emission.rateOverTime = settings.emissionRate;

            var shape = particleSystem.shape;
            shape.shapeType = ResolveShape(settings.shape);
            shape.angle = settings.shapeAngle;
            shape.radius = settings.shapeRadius;
            shape.scale = settings.shapeScale;

            var velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = settings.velocityXSet || settings.velocityYSet || settings.velocityZSet;
            if (velocity.enabled)
            {
                velocity.space = ParticleSystemSimulationSpace.Local;
                velocity.x = ToCurve(settings.velocityXSet ? settings.velocityX : MinMax.Zero);
                velocity.y = ToCurve(settings.velocityYSet ? settings.velocityY : MinMax.Zero);
                velocity.z = ToCurve(settings.velocityZSet ? settings.velocityZ : MinMax.Zero);
            }

            var noise = particleSystem.noise;
            noise.enabled = settings.noiseEnabled;
            if (noise.enabled)
            {
                noise.strength = settings.noiseStrength;
                noise.frequency = settings.noiseFrequency;
                noise.scrollSpeed = settings.noiseScrollSpeed;
            }

            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = settings.fadeOut;
            if (settings.fadeOut)
            {
                var gradient = new Gradient();
                gradient.SetKeys(
                    new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[]
                    {
                        new GradientAlphaKey(settings.startColor.a, 0f),
                        new GradientAlphaKey(settings.startColor.a, settings.fadeStart),
                        new GradientAlphaKey(0f, 1f)
                    });
                colorOverLifetime.color = gradient;
            }

            var rotation = particleSystem.rotationOverLifetime;
            rotation.enabled = settings.rotationZSet;
            if (settings.rotationZSet) rotation.z = ToCurve(settings.rotationZ);

            var size = particleSystem.sizeOverLifetime;
            size.enabled = settings.sizeOverLifetimeSet;
            if (settings.sizeOverLifetimeSet)
            {
                size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, settings.sizeOverLifetime.min, 1f, settings.sizeOverLifetime.max));
            }

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = settings.sortingOrder;
        }

        static ParticleSystemShapeType ResolveShape(string shape)
        {
            switch (shape)
            {
                case "sphere": return ParticleSystemShapeType.Sphere;
                case "hemisphere": return ParticleSystemShapeType.Hemisphere;
                case "circle": return ParticleSystemShapeType.Circle;
                case "box": return ParticleSystemShapeType.Box;
                case "cone": return ParticleSystemShapeType.Cone;
                default:
                    throw new InvalidOperationException("--shape must be cone, sphere, hemisphere, circle, or box.");
            }
        }

        static Material ResolveMaterial(string particleName, string prefabPath, string materialPath, string texturePath, out string resolvedMaterialPath, out bool materialCreated)
        {
            materialCreated = false;
            resolvedMaterialPath = materialPath;
            if (!string.IsNullOrEmpty(materialPath))
            {
                var existing = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (existing == null) throw new InvalidOperationException("Material not found: " + materialPath);
                return existing;
            }

            var defaultMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            if (string.IsNullOrEmpty(texturePath) && defaultMaterial != null)
            {
                resolvedMaterialPath = string.Empty;
                return defaultMaterial;
            }

            var folder = Path.GetDirectoryName(prefabPath)?.Replace('\\', '/') ?? "Assets";
            resolvedMaterialPath = folder + "/" + particleName + "_Particle.mat";
            EnsureParentFolder(resolvedMaterialPath);

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Unlit/Transparent");
            if (shader == null) throw new InvalidOperationException("No supported particle shader was found.");

            var material = AssetDatabase.LoadAssetAtPath<Material>(resolvedMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, resolvedMaterialPath);
                materialCreated = true;
            }
            else
            {
                material.shader = shader;
            }

            var texture = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                ConfigureTextureImporter(texturePath);
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            }

            ConfigureTransparentParticleMaterial(material);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            else if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void ConfigureTextureImporter(string texturePath)
        {
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return;
            var changed = false;
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (changed) importer.SaveAndReimport();
        }

        static void ConfigureTransparentParticleMaterial(Material material)
        {
            if (material == null) return;

            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_SrcBlendAlpha")) material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            if (material.HasProperty("_DstBlendAlpha")) material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 0f);

            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
        }

        static bool ReplaceSceneInstance(string particleName, string prefabPath, string parentPath, Vector3 localPosition)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new InvalidOperationException("Prefab not found after save: " + prefabPath);

            Transform parent = null;
            if (!string.IsNullOrWhiteSpace(parentPath))
            {
                var parentObject = FindGameObjectByPath(parentPath.Trim());
                if (parentObject == null) throw new InvalidOperationException("Parent GameObject was not found at path: " + parentPath);
                parent = parentObject.transform;
            }

            foreach (var existing in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (existing != null && existing.name == particleName)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) throw new InvalidOperationException("PrefabUtility.InstantiatePrefab failed: " + prefabPath);

            instance.name = particleName;
            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = localPosition;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            else
            {
                instance.transform.SetPositionAndRotation(localPosition, Quaternion.identity);
                instance.transform.localScale = Vector3.one;
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return true;
        }

        private static ParticleData RemoveInstance(UnityCliBridgeRequest request)
        {
            string path = (request.objectPath ?? string.Empty).Trim();
            if (path.Length == 0)
            {
                throw new InvalidOperationException("particle remove-instance requires an exact --path.");
            }
            List<GameObject> matches = new List<GameObject>();
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (GetHierarchyPath(candidate.gameObject) == path)
                        {
                            matches.Add(candidate.gameObject);
                        }
                    }
                }
            }
            if (matches.Count != 1 || matches[0].GetComponent<ParticleSystem>() == null)
            {
                throw new InvalidOperationException("The scene path must uniquely identify a ParticleSystem instance.");
            }
            ParticleData data = new ParticleData { action = "remove-instance", particleName = matches[0].name, objectPath = path, dryRun = request.dryRun, respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) };
            if (!request.dryRun)
            {
                Scene scene = matches[0].scene;
                Undo.DestroyObjectImmediate(matches[0]);
                EditorSceneManager.MarkSceneDirty(scene);
                data.sceneInstanceRemoved = true;
            }
            data.message = request.dryRun ? "Particle instance removal validated." : "Particle scene instance removed; source assets retained.";
            return data;
        }

        static GameObject FindGameObjectByPath(string path)
        {
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    var hit = FindInHierarchy(root, path);
                    if (hit != null) return hit;
                }
            }

            return null;
        }

        static GameObject FindInHierarchy(GameObject gameObject, string path)
        {
            if (string.Equals(GetHierarchyPath(gameObject), path, StringComparison.OrdinalIgnoreCase))
            {
                return gameObject;
            }

            var transform = gameObject.transform;
            for (var index = 0; index < transform.childCount; index++)
            {
                var hit = FindInHierarchy(transform.GetChild(index).gameObject, path);
                if (hit != null) return hit;
            }

            return null;
        }

        static string GetHierarchyPath(GameObject gameObject)
        {
            var names = new List<string>();
            var current = gameObject.transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        static ParticleData BuildData(UnityCliBridgeRequest request, string particleName, string prefabPath, string materialPath, string texturePath, ParticleSettings settings, List<string> issues)
        {
            return new ParticleData
            {
                action = "create",
                particleName = particleName,
                prefabPath = prefabPath,
                materialPath = materialPath,
                texturePath = texturePath,
                shape = settings.shape,
                startColor = "#" + ColorUtility.ToHtmlStringRGBA(settings.startColor),
                duration = settings.duration,
                loop = settings.loop,
                startLifetime = settings.startLifetime,
                startSpeed = settings.startSpeed,
                startSize = settings.startSize,
                emissionRate = settings.emissionRate,
                maxParticles = settings.maxParticles,
                dryRun = request.dryRun,
                issues = issues.ToArray(),
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        static Color ParseColor(string raw, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(raw)) return fallback;
            return ColorUtility.TryParseHtmlString(raw.Trim(), out var color) ? color : fallback;
        }

        static Vector3 ParseVector3(string raw, Vector3 fallback, string flagName)
        {
            if (string.IsNullOrWhiteSpace(raw)) return fallback;
            var parts = raw.Split(',');
            if (parts.Length != 3) throw new InvalidOperationException(flagName + " requires x,y,z.");
            return new Vector3(ParseFloat(parts[0], flagName), ParseFloat(parts[1], flagName), ParseFloat(parts[2], flagName));
        }

        static MinMax ParseMinMax(string raw, string flagName, out bool isSet)
        {
            isSet = false;
            if (string.IsNullOrWhiteSpace(raw)) return MinMax.Zero;
            var parts = raw.Split(',');
            if (parts.Length != 2) throw new InvalidOperationException(flagName + " requires min,max.");
            isSet = true;
            return new MinMax(ParseFloat(parts[0], flagName), ParseFloat(parts[1], flagName));
        }

        static float ParseFloat(string raw, string flagName)
        {
            if (!float.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidOperationException(flagName + " contains an invalid number: " + raw);
            }

            return value;
        }

        static ParticleSystem.MinMaxCurve ToCurve(MinMax value)
        {
            return new ParticleSystem.MinMaxCurve(value.min, value.max);
        }

        static string SanitizeObjectName(string raw)
        {
            var name = (raw ?? string.Empty).Trim();
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c.ToString(), string.Empty);
            }

            return name;
        }

        static string NormalizeAssetPath(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            return raw.Trim().Replace('\\', '/').TrimEnd('/');
        }

        static void RequireAssetPath(string path, string flagName)
        {
            if (string.IsNullOrEmpty(path)
                || (!path.StartsWith("Assets/", StringComparison.Ordinal)
                    && !path.StartsWith("Packages/", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(flagName + " must be inside Assets/ or Packages/.");
            }
        }

        static void RequireExistingAsset(string path, string flagName)
        {
            RequireAssetPath(path, flagName);
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
            {
                throw new InvalidOperationException(flagName + " asset does not exist: " + path);
            }
        }

        static void EnsureParentFolder(string assetPath)
        {
            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent) || parent == "Assets" || parent.StartsWith("Packages/", StringComparison.Ordinal))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(parent)) return;

            var segments = parent.Split('/');
            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    var guid = AssetDatabase.CreateFolder(current, segments[i]);
                    if (string.IsNullOrEmpty(guid)) throw new InvalidOperationException("Failed to create folder: " + next);
                }
                current = next;
            }
        }

        struct MinMax
        {
            public static readonly MinMax Zero = new MinMax(0f, 0f);

            public MinMax(float min, float max)
            {
                this.min = min;
                this.max = max;
            }

            public float min;
            public float max;
        }

        sealed class ParticleSettings
        {
            public string shape;
            public Color startColor;
            public float duration;
            public bool loop;
            public float startLifetime;
            public float startSpeed;
            public float startSize;
            public float emissionRate;
            public int maxParticles;
            public Vector3 position;
            public Vector3 shapeScale;
            public float shapeRadius;
            public float shapeAngle;
            public MinMax velocityX;
            public bool velocityXSet;
            public MinMax velocityY;
            public bool velocityYSet;
            public MinMax velocityZ;
            public bool velocityZSet;
            public bool noiseEnabled;
            public float noiseStrength;
            public float noiseFrequency;
            public float noiseScrollSpeed;
            public MinMax rotationZ;
            public bool rotationZSet;
            public MinMax sizeOverLifetime;
            public bool sizeOverLifetimeSet;
            public bool fadeOut;
            public float fadeStart;
            public int sortingOrder;
        }
    }
}
