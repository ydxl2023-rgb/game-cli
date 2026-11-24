using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Oathx.Avatar.Editor
{
#pragma warning disable CS8321

    public class XFBXCoroutine : IEnumerator
    {
        private Stack<IEnumerator> execStack;

        public XFBXCoroutine(IEnumerator iterator)
        {
            execStack = new Stack<IEnumerator>();
            execStack.Push(iterator);
        }

        public bool MoveNext()
        {
            IEnumerator enumerator = execStack.Peek();
            if (enumerator.MoveNext())
            {
                object result = enumerator.Current;
                if (result != null && result is IEnumerator)
                {
                    execStack.Push((IEnumerator)result);
                }

                return true;
            }
            else
            {
                if (execStack.Count > 1)
                {
                    execStack.Pop();
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            throw new System.NotSupportedException("This Operation Is Not Supported.");
        }

        public object Current
        {
            get { return execStack.Peek().Current; }
        }

        public bool Find(IEnumerator iterator)
        {
            return execStack.Contains(iterator);
        }
    }

    public class XFBXRunner
    {
        private static readonly List<XFBXCoroutine> croutines = new List<XFBXCoroutine>();
        private static readonly List<IEnumerator> buffer = new List<IEnumerator>();

        public static IEnumerator StartCoroutine(IEnumerator enumerator)
        {
            if (croutines.Count == 0)
            {
                EditorApplication.update += Update;
            }

            buffer.Add(enumerator);

            return enumerator;
        }

        private static bool Find(IEnumerator iterator)
        {
            foreach (XFBXCoroutine editorCoroutine in croutines)
            {
                if (editorCoroutine.Find(iterator))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Update()
        {
            croutines.RemoveAll(
                coroutine => { return coroutine.MoveNext() == false; }
            );

            if (buffer.Count > 0)
            {
                foreach (IEnumerator iterator in buffer)
                {
                    if (!Find(iterator))
                    {
                        croutines.Add(new XFBXCoroutine(iterator));
                    }
                }

                buffer.Clear();
            }

            if (croutines.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }
    }

    public class XAvatorFBXPostprocessor : AssetPostprocessor
    {
        private const string ModelAssetFilter = "t:Model";

        public class InternalDirectory
        {
            public const string Art = "Art";
            public const string Resources = "Resources";
            public const string Animations = "Animations";
            public const string Generics = "Generics";
            public const string Humanoids = "Humanoids";
            public const string Mask = "Mask";
            public const string Fbx = "Fbx";
        }

        public enum FbxLoopAnimation
        {
            Idle,
            Run,
            Walk,
            Forward,
            Backward,
            Stun,
            Work,
        };

        /// <summary>
        /// Applies default model importer settings before Unity imports humanoid assets.
        /// </summary>
        private void OnPreprocessModel()
        {
            ModelImporter importer = assetImporter as ModelImporter;
            if (!importer)
                return;

            ApplyHumanoidImportDefaults(importer);
        }

        void OnPostprocessModel(GameObject go)
        {
            var setting = XAvatarEditorSetting.LoadOrCreate();
            foreach (var complite in setting.complites)
            {
                if (!IsMatchInputDirectory(complite.inputDirectory, assetPath))
                {
                    continue;
                }

                DoPostprocessModels(go, complite.importerAnimationType, false, false);
            }
        }

        private bool IsLoopAnimation(string name)
        {
            string[] arrayName = System.Enum.GetNames(typeof(FbxLoopAnimation));
            foreach (string typeName in arrayName)
            {
                if (typeName == name)
                {
                    return true;
                }
            }

            return false;
        }

        private Object GeneratePrefab(GameObject go, string path, string name)
        {
            string fullpath = path + "/" + name + ".prefab";

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, fullpath);
            GameObject.DestroyImmediate(go);

            return prefab;
        }

        private bool DoPostprocessModels(GameObject go, ModelImporterAnimationType importAnimationType, bool useMeshCollider, bool copyToResources)
        {
            ModelImporter importer = (ModelImporter)assetImporter;
            importAnimationType = ResolveImportAnimationType(importAnimationType);

            if (importer.sourceAvatar && (importer.animationType == ModelImporterAnimationType.Human || importer.animationType == ModelImporterAnimationType.Generic))
                return false;

            if (assetPath.Contains("@"))
            {
                foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    Object.DestroyImmediate(smr.sharedMesh, true);
                    Object.DestroyImmediate(smr.gameObject, true);
                }

                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (!string.IsNullOrEmpty(fileName))
                {
                    string[] split = fileName.Split('@');
                    if (split.Length > 0)
                    {
                        var index = assetPath.LastIndexOf(InternalDirectory.Animations);
                        if (index <= 0)
                            return false;

                        string fbxRoot = assetPath.Substring(0, index);

                        string fbxFile = Path.ChangeExtension(Path.Combine(fbxRoot, split[0]), InternalDirectory.Fbx);
                        if (File.Exists(fbxFile))
                        {
                            Animator avatarSource = AssetDatabase.LoadAssetAtPath<Animator>(fbxFile);
                            if (avatarSource)
                            {
                                importer.sourceAvatar = avatarSource.avatar;

                                importer.animationType = importAnimationType;
                                importer.skinWeights = ModelImporterSkinWeights.Standard;
                                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                            }

                            var maskFilePath = Path.ChangeExtension(fbxFile,
                                InternalDirectory.Mask.ToLower());
                            AvatarMask avatarMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskFilePath);
                            if (!avatarMask)
                            {
                                avatarMask = new AvatarMask();

                                AssetDatabase.CreateAsset(avatarMask, maskFilePath);
                                AssetDatabase.SaveAssets();
                            }

                            var clips = importer.defaultClipAnimations;
                            for (int i = 0; i < clips.Length; i++)
                            {
                                clips[i].maskType = ClipAnimationMaskType.CopyFromOther;
                                clips[i].maskSource = avatarMask;
                                clips[i].lockRootRotation = true;
                                clips[i].keepOriginalOrientation = true;
                                clips[i].lockRootPositionXZ = true;
                                clips[i].keepOriginalPositionXZ = true;
                                clips[i].lockRootHeightY = true;
                                clips[i].keepOriginalPositionY = true;
                                clips[i].loopTime = IsLoopAnimation(split[1]);
                            }

                            importer.SaveAndReimport();
                        }
                    }
                }
            }
            else
            {
                Animator animator = go.GetComponent<Animator>();
                if (animator)
                    animator.applyRootMotion = false;

                importer.animationType = importAnimationType;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

                XFBXRunner.StartCoroutine(TryGenerateFBXPrefab(go,
                    importAnimationType, useMeshCollider, copyToResources));
            }

            return true;
        }

        /// <summary>
        /// Resolves the importer animation type for the current asset path.
        /// </summary>
        /// <param name="configuredType">The animation type configured by the Avatar editor setting.</param>
        /// <returns>The animation type that should be applied to the current asset.</returns>
        private ModelImporterAnimationType ResolveImportAnimationType(ModelImporterAnimationType configuredType)
        {
            if (IsHumanoidsAssetPath(assetPath))
                return ModelImporterAnimationType.Human;

            return configuredType;
        }

        /// <summary>
        /// Applies the humanoid defaults expected by assets stored under Humanoids folders.
        /// </summary>
        /// <param name="importer">The model importer that owns the current asset.</param>
        private void ApplyHumanoidImportDefaults(ModelImporter importer)
        {
            if (!IsHumanoidsAssetPath(assetPath))
                return;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.skinWeights = ModelImporterSkinWeights.Standard;

            if (importer.sourceAvatar)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                return;
            }

            if (TryFindHumanoidSourceAvatar(assetPath, out UnityEngine.Avatar sourceAvatar))
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = sourceAvatar;
                return;
            }

            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }

        /// <summary>
        /// Checks whether the path belongs to a humanoid asset folder.
        /// </summary>
        /// <param name="path">The asset path to inspect.</param>
        /// <returns>True when the path contains a Humanoids folder; otherwise false.</returns>
        private static bool IsHumanoidsAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return NormalizeAssetPath(path).IndexOf(InternalDirectory.Humanoids, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Finds a source avatar from the humanoid character root that owns the current asset.
        /// </summary>
        /// <param name="currentAssetPath">The asset path currently being imported.</param>
        /// <param name="sourceAvatar">The avatar found for the character root.</param>
        /// <returns>True when a source avatar is found; otherwise false.</returns>
        private static bool TryFindHumanoidSourceAvatar(string currentAssetPath, out UnityEngine.Avatar sourceAvatar)
        {
            sourceAvatar = null;

            if (!TryGetHumanoidCharacterRoot(currentAssetPath, out string characterRoot))
                return false;

            string normalizedCurrentPath = NormalizeAssetPath(currentAssetPath);
            string[] modelGuids = AssetDatabase.FindAssets(ModelAssetFilter, new[] { characterRoot });
            foreach (string modelGuid in modelGuids)
            {
                string modelPath = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(modelGuid));
                if (string.Equals(modelPath, normalizedCurrentPath, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsDirectChildAsset(characterRoot, modelPath))
                    continue;

                sourceAvatar = LoadAvatarFromModel(modelPath);
                if (sourceAvatar)
                    return true;
            }

            foreach (string modelGuid in modelGuids)
            {
                string modelPath = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(modelGuid));
                if (string.Equals(modelPath, normalizedCurrentPath, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                sourceAvatar = LoadAvatarFromModel(modelPath);
                if (sourceAvatar)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves the humanoid character root from an asset path.
        /// </summary>
        /// <param name="path">The asset path to inspect.</param>
        /// <param name="characterRoot">The resolved humanoid character root path.</param>
        /// <returns>True when a character root can be resolved; otherwise false.</returns>
        private static bool TryGetHumanoidCharacterRoot(string path, out string characterRoot)
        {
            characterRoot = null;

            string normalizedPath = NormalizeAssetPath(path);
            string[] parts = normalizedPath.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.Equals(parts[i], InternalDirectory.Humanoids, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 >= parts.Length)
                    return false;

                characterRoot = string.Join("/", parts, 0, i + 2);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether a model asset is directly under the supplied character root.
        /// </summary>
        /// <param name="characterRoot">The humanoid character root path.</param>
        /// <param name="modelPath">The model asset path to inspect.</param>
        /// <returns>True when the model is a direct child of the root; otherwise false.</returns>
        private static bool IsDirectChildAsset(string characterRoot, string modelPath)
        {
            string modelDirectory = NormalizeAssetPath(Path.GetDirectoryName(modelPath));
            return string.Equals(modelDirectory, NormalizeAssetPath(characterRoot), System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Loads the avatar sub-asset from a model asset path.
        /// </summary>
        /// <param name="modelPath">The model asset path to load from.</param>
        /// <returns>The first avatar sub-asset when available; otherwise null.</returns>
        private static UnityEngine.Avatar LoadAvatarFromModel(string modelPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is UnityEngine.Avatar avatar)
                    return avatar;
            }

            return null;
        }

        /// <summary>
        /// 判断当前导入的资源路径是否位于配置的输入目录下。
        /// </summary>
        /// <param name="inputDirectory">导入规则配置的输入目录。</param>
        /// <param name="assetPath">当前正在导入的资源路径。</param>
        /// <returns>资源路径位于输入目录下时返回 true，否则返回 false。</returns>
        private static bool IsMatchInputDirectory(string inputDirectory, string assetPath)
        {
            if (string.IsNullOrEmpty(inputDirectory))
            {
                return false;
            }
            var normalizedInputDirectory = NormalizeAssetPath(inputDirectory);
            return NormalizeAssetPath(assetPath).IndexOf(normalizedInputDirectory, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Normalizes a Unity asset path for comparisons.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The path using Unity-style separators.</returns>
        private static string NormalizeAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return path.Replace('\\', '/');
        }

        private IEnumerator TryGenerateFBXPrefab(GameObject go, ModelImporterAnimationType importAnimationType, bool useMeshCollider, bool copyToResources)
        {
            string name = Path.GetFileNameWithoutExtension(assetPath);
            if (string.IsNullOrEmpty(name))
                yield break;

            yield return new WaitForEndOfFrame();

            GameObject loadGo = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!loadGo)
                throw new System.ArgumentNullException(assetPath);

            GameObject prefabGo = new GameObject(name);
            if (useMeshCollider)
            {
                prefabGo.AddComponent<MeshCollider>();
            }
            else
            {
                CapsuleCollider collider = prefabGo.AddComponent<CapsuleCollider>();
                if (collider)
                {
                    collider.center = new Vector3(0, 1, 0);
                    collider.height = 2f;
                }
            }

            GameObject rendererGo = GameObject.Instantiate(loadGo);
            rendererGo.name = typeof(Renderer).Name;

            rendererGo.transform.SetParent(prefabGo.transform, false);
            rendererGo.transform.localScale = Vector3.one;
            rendererGo.transform.localPosition = Vector3.zero;
            rendererGo.transform.localEulerAngles = Vector3.zero;

            //var animator = rendererGo.GetComponent<Animator>();
            //if (animator)
            //    GameObject.DestroyImmediate(animator);

            string outpath = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(outpath))
                Directory.CreateDirectory(outpath);

            GeneratePrefab(prefabGo, outpath, name);
        }
    }
#pragma warning restore CS8321
}
