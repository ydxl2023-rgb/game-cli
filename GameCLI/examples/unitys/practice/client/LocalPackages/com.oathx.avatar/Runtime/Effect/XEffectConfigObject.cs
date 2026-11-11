using UnityEngine;
using System.Collections;

namespace Oathx.Avatar
{
    [CreateAssetMenu]
    public class XEffectConfigObject : ScriptableObject
    {
        /// <summary>
        /// current effect files
        /// </summary>
        public string[] effectFiles = new string[] {string.Empty};

        /// <summary>
        /// effect link bone
        /// </summary>
        public string linkBone = typeof(Renderer).Name.ToLower();

        /// <summary>
        /// effect follow modle
        /// </summary>
        public EffectFollowType follow = EffectFollowType.Both;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetEffectFile(int index)
        {
            var idx = index < effectFiles.Length ? index : effectFiles.Length - 1;
            return effectFiles[idx];
        }
    }
}
