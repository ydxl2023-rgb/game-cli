using UnityEngine;

namespace Oathx.Avatar
{
    [CreateAssetMenu]
    public class XAvatarWarnFrameObject : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public string AssetGUID = string.Empty;

        [SerializeField]
        public string attackName = string.Empty;

        public Material material;
    }
}
