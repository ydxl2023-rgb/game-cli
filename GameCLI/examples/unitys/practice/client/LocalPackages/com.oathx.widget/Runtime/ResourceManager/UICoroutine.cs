using UnityEngine;
using System.Collections;

namespace Oathx.Widget
{
    /// <summary>
    /// 
    /// </summary>
    public class UICoroutine
    {
        /// <summary>
        /// 
        /// </summary>
        private static UICoreCoroutine core = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Coroutine Run(IEnumerator function)
        {
            if (!core)
            {
                GameObject target = new GameObject(typeof(UICoreCoroutine).Name);

                target.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(target);

                core = target.AddComponent<UICoreCoroutine>();
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
