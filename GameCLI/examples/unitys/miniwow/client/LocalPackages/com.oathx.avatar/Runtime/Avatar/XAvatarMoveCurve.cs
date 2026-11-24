using UnityEngine;

namespace Oathx.Avatar
{
    [System.Serializable]
    public class XAvatarMoveCurve : ScriptableObject
    {
        public float endTime = 0;
        public AnimationCurve curve = new AnimationCurve();
    }
}
