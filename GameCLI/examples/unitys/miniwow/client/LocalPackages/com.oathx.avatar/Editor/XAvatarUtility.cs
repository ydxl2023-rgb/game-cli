using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;
using System.Text;

namespace Oathx.Avatar.Editor
{
    public static class XAvatarUtility
    {
        static public bool DrawHeader(string text)
        {
            return DrawHeader(text, text, false);
        }

        static public bool DrawHeader(string text, string key)
        {
            return DrawHeader(text, key, false);
        }

        static public bool DrawHeader(string text, bool forceOn)
        {
            return DrawHeader(text, text, forceOn);
        }

        static public bool DrawHeader(string text, string key, bool forceOn)
        {
            bool state = EditorPrefs.GetBool(key, true);

            GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);

            GUI.changed = false;
#if UNITY_3_5
            if (state) text = "\u25B2 " + text;
            else text = "\u25BC " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
#else
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25B2 " + text;
            else text = "\u25BC " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
#endif
            if (GUI.changed) EditorPrefs.SetBool(key, state);

            GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }

        static public void DrawTitle(string title)
        {
            string text = "<b><size=11>" + title + "</size></b>";
            GUILayout.Label(text, "dragtab", GUILayout.MinWidth(20f));
        }

        static public void BeginContents()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal("box", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        static public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }

        public static bool TryFindAssets<T>(out List<T> results) where T : Object
        {
            results = new List<T>();

            string[] assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (assets.Length <= 0)
                return false;

            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    var assetGo = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (assetGo)
                        results.Add(assetGo);
                }
            }

            return true;
        }

        public static string MakeLibraryPath(string name)
        {
            var dataPath = System.IO.Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/" + name + ".dat";

            return dataPath;
        }

        public static void MarkCurrentSceneDirty()
        {
            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<GameObject> GetGameObjectsHaveComponent<T>(GameObject parent) where T : Component
        {
            List<GameObject> gos = new List<GameObject>();
            if (parent.GetComponent<T>() != null)
                gos.Add(parent);
            for (int i = 0; i < parent.transform.childCount; ++i)
            {
                gos.AddRange(GetGameObjectsHaveComponent<T>(parent.transform.GetChild(i).gameObject));
            }
            return gos;
        }

        public class InternalDirectoy
        {
            public const string Resources = "Resources";
            public const string Humanoids = "Humanoids";
            public const string Animations = "Animations";            
            public const string AnimatorController = "AnimatorController";
            public const string Assets = "Assets";
            public const string Elements = "Elements";
            public const string Prefabs = "Prefabs";
            public const string Attachments = "Attachments";
            public const string Fbx = "fbx";
        }

        public static List<string> GetAssetPathInFolder(string folderPath, bool isAllDir = true, string fileFix = "*.prefab")
        {
            string fullPath = Path.Combine(Application.dataPath, folderPath);
            string[] names = Directory.GetFiles(fullPath, fileFix,
                isAllDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            List<string> fileNames = new List<string>();
            int discardeIndex = Application.dataPath.Length + 1;
            if (names != null && names.Length > 0)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    int length = names[i].Length - discardeIndex;
                    names[i] = names[i].Substring(discardeIndex, length);
                    names[i] = names[i].Replace("\\", "/");

                    fileNames.Add(Path.Combine(InternalDirectoy.Assets.ToString(), names[i]));
                }
            }
            return fileNames;
        }

        public static void GenerateAsset(string path, Object asset)
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }

        public static void GenerateController(GameObject prefab, GameObject srcfbx, string path, AnimatorController ac, string outDir)
        {
            if (ac)
            {
                List<string> eventEffects = new List<string>();

                Animator animator = prefab.GetComponent<Animator>();
                if (!animator)
                    animator = prefab.AddComponent<Animator>();

                string directoryName = Path.GetFileNameWithoutExtension(path);
                string outPath = Path.Combine(outDir, directoryName, typeof(AnimatorOverrideController).Name.ToLower());
                if (Directory.Exists(outPath))
                    Directory.Delete(outPath);

                Directory.CreateDirectory(outPath);

                string animationPath = Path.Combine(Path.GetDirectoryName(path),
                    InternalDirectoy.Animations.ToString()).Replace("\\", "/");

                DirectoryInfo dir = new DirectoryInfo(Path.Combine(Application.dataPath, animationPath.Replace(InternalDirectoy.Assets.ToString() + "/", "")));
                FileSystemInfo[] animationSubs = dir.Exists ? dir.GetFileSystemInfos() : new FileSystemInfo[0];
                foreach (FileSystemInfo sub in animationSubs)
                {
                    if (sub is DirectoryInfo)
                    {
                        string name = Path.GetFileName(sub.FullName);

                        List<string> files = GetAssetPathInFolder(sub.FullName, true, "*.fbx");
                        if (files.Count > 0)
                        {
                            AnimatorOverrideController oac = new AnimatorOverrideController(ac);
                            if (!oac)
                                throw new System.NullReferenceException();

                            List<KeyValuePair<AnimationClip, AnimationClip>> oacClips = new List<KeyValuePair<AnimationClip, AnimationClip>>(oac.overridesCount);
                            List<KeyValuePair<AnimationClip, AnimationClip>> newClips = new List<KeyValuePair<AnimationClip, AnimationClip>>(oac.overridesCount);

                            oac.GetOverrides(oacClips);

                            for (int i = 0; i < oacClips.Count; i++)
                            {
                                string assetPath = string.Empty;
                                for (int j = 0; j < files.Count; j++)
                                {
                                    string[] splits = Path.GetFileName(files[j].ToLower()).Replace(".fbx", "").Split('@');
                                    if (splits.Length >= 2)
                                    {
                                        if (splits[1] == oacClips[i].Key.name.ToLower())
                                        {
                                            assetPath = files[j].ToLower();
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (splits[0] == oacClips[i].Key.name.ToLower())
                                        {
                                            assetPath = files[j].ToLower();
                                            break;
                                        }
                                    }

                                }

                                if (!string.IsNullOrEmpty(assetPath))
                                {
                                    KeyValuePair<AnimationClip, AnimationClip> pair = oacClips[i];
                                    KeyValuePair<AnimationClip, AnimationClip> newPair;

                                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                                    if (clip)
                                    {
                                        if (clip.name != oacClips[i].Key.name)
                                            clip.name = oacClips[i].Key.name;

                                        AnimationEvent[] events = clip.events;
                                        foreach (AnimationEvent e in events)
                                        {
                                            if (e.functionName == GetFunctionName(XAnimationEventName.Effect))
                                            {
                                                XEffectConfigObject effect = e.objectReferenceParameter as XEffectConfigObject;
                                                if (effect)
                                                {
                                                    foreach (string effectPath in effect.effectFiles)
                                                    {
                                                        bool added = true;
                                                        foreach (string old in eventEffects)
                                                        {
                                                            if (old == effectPath)
                                                            {
                                                                added = false;
                                                                break;
                                                            }
                                                        }

                                                        if (added)
                                                        {
                                                            eventEffects.Add(effectPath);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        newPair = new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, clip);
                                    }
                                    else
                                    {
                                        newPair = new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, pair.Value);
                                    }

                                    newClips.Add(newPair);
                                }
                            }

                            oac.ApplyOverrides(newClips);

                            string outName = Path.Combine(outPath, string.Format("{0}.{1}", name,
                                typeof(AnimatorOverrideController).Name.Replace(typeof(Animator).Name, "")));
                            GenerateAsset(outName, oac);

                            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(outName);
                            animator.enabled = true;

                            if (srcfbx)
                            {
                                Animator srcAnimator = srcfbx.GetComponent<Animator>();
                                if (srcAnimator)
                                {
                                    animator.avatar = srcAnimator.avatar;
                                }
                            }
                        }
                    }
                }

                XAvatarPreload preload = prefab.GetComponent<XAvatarPreload>();
                if (!preload)
                    preload = prefab.AddComponent<XAvatarPreload>();

                preload.effects = eventEffects.ToArray();
            }
        }

        public static Object GeneratePrefab(GameObject go, string path, string name)
        {
            string fullpath = path + "/" + name + ".prefab";

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, fullpath);
            GameObject.DestroyImmediate(go);

            return prefab;
        }

        public static Object ExtractSkeleton(GameObject prefab, GameObject srcfbx, string path, AnimatorController ac, string outpath)
        {
            GameObject prefabObject = GameObject.Instantiate(prefab);
            if (!prefabObject)
                throw new System.NullReferenceException(prefab.name);

            prefabObject.name = srcfbx.name;

            SkinnedMeshRenderer[] prefabRenders = prefabObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < prefabRenders.Length; i++)
            {
                GameObject.DestroyImmediate(prefabRenders[i].gameObject);
            }

            MeshRenderer[] meshRender = prefabObject.GetComponentsInChildren<MeshRenderer>(true);
            for(int i=0; i<meshRender.Length; i++)
            {
                GameObject.DestroyImmediate(meshRender[i].gameObject);
            }

            Animator animator = prefabObject.GetComponent<Animator>();
            if (!animator)
                animator = prefabObject.AddComponent<Animator>();

            // avatar 赋值不能依赖 GenerateController（controller 为空或 Animations 目录缺失时不会执行），
            // 否则下游 GetBoneTransform 会因 Avatar is null 抛异常
            if (!animator.avatar && srcfbx)
            {
                Animator srcAnimator = srcfbx.GetComponent<Animator>();
                if (srcAnimator && srcAnimator.avatar)
                    animator.avatar = srcAnimator.avatar;
            }

            // FBX 配置 Copy From Other Avatar 时模型 Animator 的 avatar 为空，从导入器配置的 sourceAvatar 兜底
            if (!animator.avatar && srcfbx)
            {
                ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(srcfbx)) as ModelImporter;
                if (importer && importer.sourceAvatar)
                    animator.avatar = importer.sourceAvatar;
            }

            string outDirentory = Path.Combine(outpath, Path.GetFileNameWithoutExtension(path));
            if (Directory.Exists(outDirentory))
                Directory.Delete(outDirentory, true);

            Directory.CreateDirectory(outDirentory);

            if (ac)
            {
                GenerateController(prefabObject, srcfbx, path, ac, outpath);
            }

            XAvatarHuman human = prefabObject.AddComponent<XAvatarHuman>();
            human.GenerateBoneNogPoint(XAvatarEditorSetting.Instance.boneNogPoints.ToArray());

            Rigidbody body = prefabObject.GetComponent<Rigidbody>();
            if (!body)
            {
                body = prefabObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            return GeneratePrefab(prefabObject, outDirentory, prefab.name);
        }

        public static void ExtractSkinMesh(GameObject prefab, GameObject srcfbx, string path, string outDirectory, bool outputDescribeFile=true)
        {
            List<string> files = GetAssetPathInFolder(
                Path.GetDirectoryName(path).Replace("\\", "/").Replace(InternalDirectoy.Assets.ToString() + "/", ""), true, "*.mat");
            if (files.Count > 0)
            {
                List<Material> materials = new List<Material>();
                foreach (string file in files)
                {
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(file);
                    if (mat)
                        materials.Add(mat);
                }

                string fileDirectoryName = Path.GetFileNameWithoutExtension(path).ToLower();
                string outpath = Path.Combine(outDirectory, fileDirectoryName);
                if (!Directory.Exists(outpath))
                    Directory.CreateDirectory(outpath);

                string prefabPaths = Path.Combine(outpath, InternalDirectoy.Prefabs.ToLower());
                if (Directory.Exists(prefabPaths))
                    Directory.Delete(prefabPaths, true);

                string elementPath = Path.Combine(outpath, InternalDirectoy.Elements.ToLower());
                if (Directory.Exists(elementPath))
                    Directory.Delete(elementPath, true);

                string attachPath = Path.Combine(outpath, InternalDirectoy.Attachments.ToLower());
                if (Directory.Exists(attachPath))
                    Directory.Delete(attachPath, true);

                Directory.CreateDirectory(prefabPaths);
                Directory.CreateDirectory(elementPath);
                Directory.CreateDirectory(attachPath);

                StringBuilder describe = new StringBuilder();

                int boneWeightIndex = 0;
                SkinnedMeshRenderer[] renders = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                for (int i = 0; i < renders.Length; i++)
                {
                    SkinnedMeshRenderer smr = renders[i];
                    if (smr)
                    {
                        List<GameObject> smrGos = new List<GameObject>();
                        foreach (Material mat in materials)
                        {
                            GameObject smrGo = GameObject.Instantiate(smr.gameObject);
                            smrGo.name = smr.name + mat.name;

                            List<Material> smrMaterials = new List<Material>();
                            smrMaterials.Add(mat);

                            Renderer[] smrRenders = smrGo.GetComponents<Renderer>();
                            foreach (var render in smrRenders)
                            {
                                render.sharedMaterials = smrMaterials.ToArray();
                            }

                            smrGos.Add(smrGo);
                        }

                        foreach (var go in smrGos)
                        {
                            Avatar.XAvatarElement element = ScriptableObject.CreateInstance<Avatar.XAvatarElement>();
                            if (element)
                            {
                                element.BoneWeights = new List<XBoneWeightRecord>();

                                List<string> boneNames = new List<string>();
                                List<Matrix4x4>
                                    bindPoses = new List<Matrix4x4>();

                                foreach (Transform t in smr.bones)
                                {
                                    boneNames.Add(t.name);
                                    bindPoses.Add(t.worldToLocalMatrix);

                                    element.BoneWeights.Add(new XBoneWeightRecord()
                                    {
                                        BoneName = t.name,
                                        WeightIndex = boneWeightIndex
                                    });

                                    boneWeightIndex++;
                                }

                                element.SmrLocalToWorldMatrix = smr.transform.localToWorldMatrix;
                                element.BoneNames = boneNames;
                                element.BindPoses = bindPoses;

                                Renderer renderer = go.GetComponent<Renderer>();
                                element.SharedMaterials = new List<Material>(renderer.sharedMaterials);

                                Object elementPrefab = GeneratePrefab(go, prefabPaths, go.name.ToLower());
                                element.name = elementPrefab.name;
                                element.Prefab = elementPrefab;
                                element.Name = elementPrefab.name;
                                element.PrefabPath = AssetDatabase.GetAssetPath(elementPrefab);

                                string outElementConfigFullPath = Path.Combine(elementPath, elementPrefab.name.ToLower() + ".asset");
                                GenerateAsset(outElementConfigFullPath, element);

                                if (outputDescribeFile)
                                    describe.AppendLine($"{{{outElementConfigFullPath.Replace("\\", "/")}}}");
                            }
                        }
                    }
                }

                if (outputDescribeFile)
                    describe.AppendLine(typeof(MeshRenderer).Name);

                MeshRenderer[] meshRenders = prefab.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var meshRender in meshRenders)
                {
                    List<GameObject> mrGos = new List<GameObject>();
                    foreach (Material mat in materials)
                    {
                        GameObject mrGo = GameObject.Instantiate(meshRender.gameObject);
                        mrGo.name = meshRender.name + mat.name;

                        List<Material> smrMaterials = new List<Material>();
                        smrMaterials.Add(mat);

                        Renderer[] smrRenders = mrGo.GetComponents<Renderer>();
                        foreach (var render in smrRenders)
                        {
                            render.sharedMaterials = smrMaterials.ToArray();
                        }

                        mrGos.Add(mrGo);
                    }

                    foreach(var go in mrGos)
                    {
                        GameObject attackGo = new GameObject(go.name);

                        go.transform.SetParent(attackGo.transform, false);
                        go.transform.localPosition = meshRender.transform.localPosition;
                        go.transform.localScale = meshRender.transform.localScale;
                        go.transform.localEulerAngles = meshRender.transform.localEulerAngles;

                        var attackPrefab = GeneratePrefab(attackGo, prefabPaths, go.name.ToLower());

                        XAvatarAttachElement element = ScriptableObject.CreateInstance<XAvatarAttachElement>();
                        element.Prefab = attackPrefab;
                        element.name = meshRender.name;
                        element.Name = meshRender.name;
                        element.BoneNogPointName = meshRender.transform.parent.name;
                        element.PrefabPath = AssetDatabase.GetAssetPath(attackPrefab);
                        
                        string outPartConfigFullPath = Path.Combine(attachPath, attackPrefab.name.ToLower() + ".asset");
                        GenerateAsset(outPartConfigFullPath, element);

                        if (outputDescribeFile)
                            describe.AppendLine($"{{{outPartConfigFullPath.Replace("\\", "/")}}}");
                    }
                }

                string text = describe.ToString();
                if (!string.IsNullOrEmpty(text))
                    File.WriteAllText(elementPath + ".bytes", text);
            }
        }

        public static string GetFunctionName(XAnimationEventName type)
        {
            string name = string.Empty;
            switch (type)
            {
                case XAnimationEventName.Event:
                    name = "OnAnimationEvent";
                    break;
                case XAnimationEventName.AttackStart:
                    name = "OnAnimationAttackStart";
                    break;
                case XAnimationEventName.AttackEnd:
                    name = "OnAnimationAttackEnd";
                    break;
                case XAnimationEventName.AttackWarnStart:
                    name = "OnAnimationAttackWarnStart";
                    break;
                case XAnimationEventName.AttackWarnEnd:
                    name = "OnAnimationAttackWarnEnd";
                    break;
                case XAnimationEventName.Sound:
                    name = "OnAnimationSound";
                    break;
                case XAnimationEventName.Effect:
                    name = "OnAnimationEffect";
                    break;
                case XAnimationEventName.RenderSetting:
                    name = "OnAnimationRenderSetting";
                    break;
                case XAnimationEventName.ForwardStart:
                    name = "OnAnimationForwardStart";                    
                    break;
                case XAnimationEventName.ForwardEnd:
                    name = "OnAnimationForwardEnd";
                    break;
                case XAnimationEventName.BackwardStart:
                    name = "OnAnimationBackwardStart";
                    break;
                case XAnimationEventName.BackwardEnd:
                    name = "OnAnimationBackwardEnd";
                    break;
                case XAnimationEventName.JumpStart:
                    name = "OnAnimationJumpStart";
                    break;
                case XAnimationEventName.JumpEnd:
                    name = "OnAnimationJumpEnd";
                    break;
            }

            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XAnimationEventName GetFunctionType(string name)
        {
            XAnimationEventName type = XAnimationEventName.None;
            switch (name)
            {
                case "OnAnimationEvent":
                    type = XAnimationEventName.Event;
                    break;
                case "OnAnimationAttackStart":
                    type = XAnimationEventName.AttackStart;
                    break;
                case "OnAnimationAttackEnd":
                    type = XAnimationEventName.AttackEnd;
                    break;
                case "OnAnimationAttackWarnStart":
                    type = XAnimationEventName.AttackWarnStart;
                    break;
                case "OnAnimationAttackWarnEnd":
                    type = XAnimationEventName.AttackWarnEnd;
                    break;
                case "OnAnimationSound":
                    type = XAnimationEventName.Sound;
                    break;
                case "OnAnimationEffect":
                    type = XAnimationEventName.Effect;
                    break;
                case "OnAnimationRenderSetting":
                    type = XAnimationEventName.RenderSetting;
                    break;
                case "OnAnimationForwardStart":
                    type = XAnimationEventName.ForwardStart;
                    break;
                case "OnAnimationForwardEnd":
                    type = XAnimationEventName.ForwardEnd;
                    break;
                case "OnAnimationBackwardStart":
                    type = XAnimationEventName.BackwardStart;
                    break;
                case "OnAnimationBackwardEnd":
                    type = XAnimationEventName.BackwardEnd;
                    break;
                case "OnAnimationJumpStart":
                    type = XAnimationEventName.JumpStart;
                    break;
                case "OnAnimationJumpEnd":
                    type = XAnimationEventName.JumpEnd;
                    break;
            }

            return type;
        }
    }
}
