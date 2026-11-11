using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Oathx.Avatar
{
    public enum EffectEventType
    {
        Finished,
        Stoped,
        TriggerNext,
    };

    public enum EffectFollowType
    {
        None, 
        Both, 
        Position, 
        PositionButY, 
        RotButY, 
        Flip
    };

    public class XEffectComponent : MonoBehaviour
    {
        [HideInInspector]
        public int gid = 0;

        [HideInInspector]
        public GameObject owner;

        [HideInInspector]
        public string resPath;

        public bool forceUnfollow = false;
        [HideInInspector, System.NonSerialized]
        public bool onlyFollowPosition = false;
        [HideInInspector, System.NonSerialized]
        public bool ignoreFollowHeight = false;
        [HideInInspector, System.NonSerialized]
        public float followStayHeight = .0f;

        public bool isOneShot = false;
        public bool keepLooping = false;
        public float playTime = 5.0f;
        public float triggerNextTime = 3.0f;
        public bool autoDestroy = true;

        public delegate void EffectEventHandler(XEffectComponent effect, EffectEventType eventType);
        public EffectEventHandler eventCallBack;

        private float startTime;
        private bool reachTrigger = false;
        private ParticleSystem[] shurikens;

        void Awake()
        {
            shurikens = gameObject.GetComponentsInChildren<ParticleSystem>(); ;
        }

        void OnEnable()
        {
            startTime = Time.time;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDestroy()
        {
            if (gid > 0)
                XEffectManager.GetSingleton().DestroyEffect(this, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="effectPrefab"></param>
        /// <param name="time"></param>
        public void Initialize(GameObject parent, GameObject effectPrefab, float time)
        {
            if (parent != null)
            {
                transform.position = parent.transform.position;
                if (onlyFollowPosition)
                    transform.rotation = effectPrefab.transform.rotation;
                else
                    transform.rotation = parent.transform.rotation;
            }

            transform.Rotate(effectPrefab.transform.rotation.eulerAngles);
            if (parent != null)
                transform.SetParent((forceUnfollow || onlyFollowPosition) ? null : parent.transform);
            transform.localScale = effectPrefab.transform.localScale;

            playTime = time;
            owner = parent;
            isOneShot = (time == 0);

            if (time < 0)
                keepLooping = true;
            else
                keepLooping = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="effectPrefab"></param>
        /// <param name="time"></param>
        public void Initialize(GameObject parent,
            Vector3 position, Quaternion rotation, GameObject effectPrefab, float time)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.SetParent(null);
            transform.Rotate(effectPrefab.transform.rotation.eulerAngles);
            transform.Translate(effectPrefab.transform.position, Space.World);
            transform.localScale = effectPrefab.transform.localScale;

            playTime = time;
            owner = parent;
            isOneShot = (time == 0);

            if (time < 0)
                keepLooping = true;
            else
                keepLooping = false;
        }

        void Update()
        {
            if (isOneShot)
            {
                if (IsOneShotParticleSystemFinished())
                {
                    if (eventCallBack != null)
                        eventCallBack(this, EffectEventType.Finished);

                    Destroy();
                }
            }
            else
            {
                float elapseTime = Time.time - startTime;
                if (!reachTrigger && elapseTime > triggerNextTime)
                {
                    reachTrigger = true;

                    if (eventCallBack != null)
                        eventCallBack(this, EffectEventType.TriggerNext);
                }

                if (elapseTime > playTime && !keepLooping)
                {
                    if (eventCallBack != null)
                        eventCallBack(this, EffectEventType.Finished);

                    Destroy();
                }
            }
        }

        void LateUpdate()
        {
            if (owner != null)
            {
                if (onlyFollowPosition)
                {
                    if (ignoreFollowHeight)
                        transform.position = new Vector3(owner.transform.position.x, followStayHeight, owner.transform.position.z);
                    else
                        transform.position = owner.transform.position;
                }

                if (!owner.activeInHierarchy)
                    Destroy();
            }

        }

        public void Stop()
        {
            if (eventCallBack != null)
                eventCallBack(this, EffectEventType.Stoped);

            Destroy();
        }

        void Destroy()
        {
            if (!string.IsNullOrEmpty(resPath))
            {
                XEffectManager.GetSingleton().DestroyEffect(this);
            }
            else
            {
                gameObject.SetActive(false);

                if (autoDestroy)
                    XEffectManager.GetSingleton().DestroyEffect(this);
            }
        }

        bool IsOneShotParticleSystemFinished()
        {
            bool isFinished = true;
            for (int i = 0; i < shurikens.Length; i++)
            {
                if (shurikens[i] != null)
                {
                    if (shurikens[i].isPlaying)
                        return false;

                    isFinished &= !shurikens[i].isPlaying;
                }
            }

            if (isFinished)
            {
                for (int i = 0; i < shurikens.Length; i++)
                {
                    if (shurikens[i] != null && shurikens[i].IsAlive())
                        return false;
                }
            }

            return isFinished;
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Generate Effect Config")]
        protected static void GenerateEffectConfig()
        {
            foreach(var prefab in Selection.objects)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefab).ToLower();
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogError("Can't find object path");
                }
                else
                {
                    string assetPath = prefabPath.Replace("arts", "datas").Replace(".prefab", ".asset").Replace("prefabs", "configs");

                    XEffectConfigObject config = ScriptableObject.CreateInstance<XEffectConfigObject>();
                    config.effectFiles[0] = prefabPath;

                    string directory = Path.GetDirectoryName(assetPath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    XEffectConfigObject effect = AssetDatabase.LoadAssetAtPath<XEffectConfigObject>(assetPath);
                    if (effect)
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        AssetDatabase.SaveAssets();
                    }

                    AssetDatabase.CreateAsset(config, assetPath);

                    Debug.Log($"Create XEffect Config {assetPath}");
                }
            }
            
        }
#endif
    }

}
