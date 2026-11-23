using UnityEngine;
using System.Collections;

namespace Oathx.Avatar
{
    public class XAvatarCoroutine
    {
        private static XAvatarCoreCoroutine core = null;

        public static Coroutine Run(IEnumerator function)
        {
            if (!core)
            {
                GameObject target = new GameObject(typeof(XAvatarCoreCoroutine).Name);

                target.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(target);

                core = target.AddComponent<XAvatarCoreCoroutine>();
            }

            return core.StartCoroutine(function);
        }

        public static void StopAll()
        {
            if (!core)
                core.StopAllCoroutines();
        }

        public static void StopCoroutine(Coroutine c)
        {
            if (!core)
                core.StopCoroutine(c);
        }
    }
}
