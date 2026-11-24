using UnityEngine;

namespace Oathx.Avatar
{
    public enum XAttachDimension
    {
        Normal,
        Big,
        Small,
        Middle,
    }

    public class XAvatarAttachElement : ScriptableObject
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public string BoneNogPointName;

        [SerializeField]
        public Object Prefab;

        [SerializeField]
        public string PrefabPath;
    }

}
