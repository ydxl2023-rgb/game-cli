using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    public enum XHitReactAnimationType
    {
        None,
        Hit,
        BlockHit,
        HeavyHit,
    }

    [System.Serializable]
    public class XHitReact
    {
        [HideInInspector, System.NonSerialized]
        public Quaternion hitPointRot;
        [HideInInspector, System.NonSerialized]
        public Vector3 hitPoint;

        [Header("Audio")]
        public AudioClip hitAudioClip;

        [Header("Effects [level, effect]")]
        public XEffectConfigObject[] hitEffects;
    }

    public enum XAttackCheckType
    {
        Normal,
        Bullet,
    }

    [CreateAssetMenu]
    public class XAvatarAttackFrame : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public string AssetGUID = string.Empty;

        [HideInInspector, System.NonSerialized]
        public string attackName = string.Empty;

        public HumanBodyBones bone;
        public Vector3 position;
        public Quaternion rotation;
        public float range;

        [Header("Check Type")]
        public XAttackCheckType checkType = XAttackCheckType.Normal;
        public GameObject bulletPrefab;
        [HideInInspector]
        public string fireNogPointName;
        // 0.528 -0.965 0.094
        public Vector3 fireOffset;

        [System.NonSerialized]
        public int dmgCurrent = 1;
        [System.NonSerialized]
        public int dmgTotal = 1;
        [System.NonSerialized]
        public int dmgRate = 100;

        [Header("Hit Radial Blur")]
        public bool blur = false;
        public float blurDist = 0.1f;
        public float blurStrength = 3.5f;
        public int blurFrame = 3;

        [Header("Hit react")]
        public XHitReact react;
    }
}

