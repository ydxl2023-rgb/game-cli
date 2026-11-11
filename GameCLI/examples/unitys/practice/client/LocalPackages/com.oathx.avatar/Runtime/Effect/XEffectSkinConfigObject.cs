using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Oathx.Avatar
{
    [CreateAssetMenu]
    public class XEffectSkinConfigObject : ScriptableObject
    {
        public Material skinMaterial;
        public List<string> skinParam = new List<string>();
        public AnimationCurve skinCurve = new AnimationCurve();
        public bool isMutiply = false;

        public string[] GetSkinParams()
        {
            return skinParam.ToArray();
        }
    }
}
