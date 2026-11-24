using UnityEngine;
using System.Collections;

namespace Oathx.Core
{
    /// <summary>
    /// coroutine helper
    /// </summary>
    public class XCoroutine
    {
        /// <summary>
        /// coroutine core
        /// </summary>
        private static XCoreCoroutine core = null;

        /// <summary>
        /// run coroutine
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Coroutine Run(IEnumerator function)
        {
            if (!core)
            {
                GameObject target = new GameObject(typeof(XCoreCoroutine).Name);

                target.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(target);

                core = target.AddComponent<XCoreCoroutine>();
            }

            return core.StartCoroutine(function);
        }

        /// <summary>
        /// stop all coroutines
        /// </summary>
        public static void StopAll()
        {
            if (core)
                core.StopAllCoroutines();
        }

        /// <summary>
        /// stop coroutine
        /// </summary>
        /// <param name="c"></param>
        public static void StopCoroutine(Coroutine c)
        {
            if (core)
                core.StopCoroutine(c);
        }
    }
}
