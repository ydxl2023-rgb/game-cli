using UnityEngine;
using System.Collections;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// coroutine runner
    /// </summary>
    public class XBundleCoroutine
    {
        private static XBundleCoreCoroutine core = null;

        /// <summary>
        /// run coroutine
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Coroutine Run(IEnumerator function)
        {
            if (!core)
            {
                GameObject target = new GameObject(typeof(XBundleCoreCoroutine).Name);

                target.hideFlags = HideFlags.HideAndDontSave;
                Object.DontDestroyOnLoad(target);

                core = target.AddComponent<XBundleCoreCoroutine>();
            }

            return core.StartCoroutine(function);
        }

        /// <summary>
        /// stop all coroutines
        /// </summary>
        public static void StopAll()
        {
            if (!core)
                core.StopAllCoroutines();
        }

        /// <summary>
        /// stop coroutine
        /// </summary>
        /// <param name="c"></param>
        public static void StopCoroutine(Coroutine c)
        {
            if (!core)
                core.StopCoroutine(c);
        }
    }
}
