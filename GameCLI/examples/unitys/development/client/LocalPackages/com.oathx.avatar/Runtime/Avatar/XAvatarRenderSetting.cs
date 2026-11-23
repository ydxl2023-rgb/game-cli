using UnityEngine;

namespace Oathx.Avatar
{
    [CreateAssetMenu]
    public class XAvatarRenderSetting : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public string AssetGUID = string.Empty;

        [SerializeField]
        public string clipName = string.Empty;

        public Material material;
    }
}
