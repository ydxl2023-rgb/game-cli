using UnityEngine;
using System.Collections.Generic;

namespace Oathx.Avatar
{
    public sealed class XBulletManager
    {
        class BulletCache
        {
            public string path;
            public GameObject prefab;
        }

        static Queue<BulletCache> bulletCache = new Queue<BulletCache>();
        static readonly int MaxCacheCount = 10;

        public static void AddBulletCache(string path, GameObject prefab)
        {
            bool added = true;
            foreach (BulletCache cache in bulletCache)
            {
                if (cache.path == path)
                {
                    added = false;
                    break;
                }
            }

            if (added)
            {
                if (bulletCache.Count >= MaxCacheCount)
                {
                    BulletCache cache = bulletCache.Dequeue();
                    if (cache.prefab)
                    {
                        GameObject.Destroy(cache.prefab);
                    }
                }

                bulletCache.Enqueue(new BulletCache()
                {
                    path = path,
                    prefab = prefab
                });
            }
        }

        static BulletCache GetBulletCache(string path)
        {
            foreach (BulletCache cache in bulletCache)
            {
                if (cache.path == path)
                {
                    return cache;
                }
            }

            return null;
        }

        static void ClearBulletCache()
        {
            foreach (BulletCache cache in bulletCache)
            {
                if (cache.prefab)
                {
                    GameObject.Destroy(cache.prefab);
                }
            }

            bulletCache.Clear();
        }

        static bool ExistCache(string path)
        {
            foreach (BulletCache cache in bulletCache)
            {
                if (cache.path == path)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GenerateBullet(GameObject prefab,
            Vector3 position, Transform target, float speed, System.Func<XBulletComponent, Transform, bool> callback)
        {
            GameObject bulletGo = Object.Instantiate(prefab);
            bulletGo.transform.position = position;

            XBulletComponent bullet = bulletGo.GetComponent<XBulletComponent>();
            if (!bullet)
                throw new System.NullReferenceException();

            bullet.Initialize(position, target.position, speed, target, callback);
            return true;
        }

        public static bool GenerateBullet(GameObject prefab,
               Transform fireTrans, Vector3 targetPosition, float speed, System.Func<XBulletComponent, Transform, bool> callback)
        {
            GameObject bulletGo = Object.Instantiate(prefab);
            if (!bulletGo)
                return false;

            XBulletComponent bullet = bulletGo.GetComponent<XBulletComponent>();
            if (!bullet)
                throw new System.NullReferenceException();

            bullet.Initialize(fireTrans, targetPosition, speed, null, callback);
            return true;
        }

        public static bool GenerateBullet(GameObject prefab,
            Vector3 position, Vector3 targetPosition, float speed, System.Func<XBulletComponent, Transform, bool> callback)
        {
            GameObject bulletGo = Object.Instantiate(prefab);
            if (!bulletGo)
                return false;

            XBulletComponent bullet = bulletGo.GetComponent<XBulletComponent>();
            if (!bullet)
                throw new System.NullReferenceException();

            bullet.Initialize(position, targetPosition, speed, null, callback);

            return true;
        }

        public static bool GenerateAsyncBullet(string path,
            Vector3 position, Vector3 destPosition, float speed, Transform target, System.Func<XBulletComponent, Transform, bool> callback)
        {
            BulletCache cache = GetBulletCache(path);
            if (cache == null)
            {
                XEffectResourceManager.GetSingleton().LoadAsync<GameObject>(path, delegate (Object prefab)
                {
                    var go = prefab as GameObject;
                    AddBulletCache(path, go);

                    GenerateBullet(go, position, destPosition, speed, callback, target);
                });
            }
            else
            {
                GenerateBullet(cache.prefab, position, destPosition, speed, callback, target);
            }

            return true;
        }

        public static bool GenerateBullet(GameObject prefab,
            Vector3 position, Vector3 destPosition, float speed, System.Func<XBulletComponent, Transform, bool> callback, Transform target)
        {
            GameObject bulletGo = Object.Instantiate(prefab);
            if (!bulletGo)
                return false;

            XBulletComponent bullet = bulletGo.GetComponent<XBulletComponent>();
            if (!bullet)
                bullet = bulletGo.AddComponent<XBulletComponent>();

            bullet.Initialize(position, destPosition, speed, target, callback);

            return true;
        }
    }
}
