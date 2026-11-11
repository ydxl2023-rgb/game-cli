using System.Collections;
using UnityEngine;

namespace Oathx.Avatar
{
  
    /// <summary>
    /// 
    /// </summary>
    public class XEffectCoroutine
    {
        /// <summary>
        /// 
        /// </summary>
        private static XEffectCoreCoroutine core = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Coroutine Run(IEnumerator function)
        {
            if (!core)
            {
                GameObject target = new GameObject(typeof(XEffectCoreCoroutine).Name);

                target.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(target);

                core = target.AddComponent<XEffectCoreCoroutine>();
            }

            return core.StartCoroutine(function);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StopAll()
        {
            if (!core)
                core.StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        public static void StopCoroutine(Coroutine c)
        {
            if (!core)
                core.StopCoroutine(c);
        }
    }
    

}
