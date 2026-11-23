using System.Collections.Generic;
using System;
using UnityEngine;

namespace Oathx.Entity
{

#if UNITY_EDITOR
    /// <summary>
    /// entity utility
    /// </summary>
    public static class XEntityEditorUtility
    {
        /// <summary>
        /// entity changed event
        /// </summary>
        public static event Action<XEntity> EntityChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public static void Notify(XEntity entity)
        {
            EntityChanged?.Invoke(entity);
        }
    }
#endif

    public enum InstallType
    {
        Startup,
        Custom
    }

    /// <summary>
    /// auto create on startup attribute
    /// </summary>
    public class Initialize : Attribute
    {
        /// <summary>
        /// components, if components is not null, then add components to entity, otherwise just create entity without component
        /// </summary>
        public List<Type> Components
        { get; private set; } = new List<Type>();

        /// <summary>
        /// tick, if tick is true, then update entity every frame, otherwise just update entity when property changed, default is false
        /// </summary>
        public bool EveryFrameUpdate
        { get; set; } = false;

        /// <summary>
        /// install type, default is custom, if install type is startup,
        /// then create entity on startup
        /// otherwise just create entity when add entity to entity manager
        /// </summary>
        public InstallType InstallType
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoents"></param>
        public Initialize(InstallType installType=InstallType.Startup, bool everyFrameUpdate =false, Type[] typeCompoents = null) 
        {
            EveryFrameUpdate = everyFrameUpdate;
            InstallType = installType;

            if (typeCompoents != null)
                Components.AddRange(typeCompoents);
        }
    }

    /// <summary>
    /// entity manager
    /// </summary>
    public class XEntityManager
    {
        private static readonly XEntityManager instance = new XEntityManager();

        /// <summary>
        /// get singleton
        /// </summary>
        /// <returns></returns>
        public static XEntityManager GetSingleton()
        {
            return instance;
        }

        /// <summary>
        /// entitys
        /// </summary>
        private readonly Dictionary<string, XEntity> entitys = new Dictionary<string, XEntity>();

        /// <summary>
        /// 
        /// </summary>
        public List<XEntity> frameEntitys
        { get; private set; } = new List<XEntity>();

        /// <summary>
        /// startup
        /// </summary>
        /// <param name="onStartupTypes"></param>
        /// <returns></returns>
        public bool Startup(List<Type> onStartupTypes)
        {
            foreach (var type in onStartupTypes)
            {
                var entity = CreateEntity(type);
                if (!entity)
                    throw new NullReferenceException(type.FullName);

                var initialize = type.GetCustomAttributes(typeof(Initialize), true);
                foreach(var attr in initialize)
                {
                    if (attr is Initialize ci)
                    {
                        foreach (var component in ci.Components)
                        {
                            entity.AddComponent(component);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// destroy all entitys
        /// </summary>
        public void Shutdown()
        {
            // clear frame entitys
            frameEntitys.Clear();

            foreach (var it in entitys)
            {
                it.Value.Destroy();
            }

            entitys.Clear();
        }

        /// <summary>
        /// creat entity
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public XEntity CreateEntity(Type type)
        {
            string typeName = type.Name;
            if (entitys.ContainsKey(typeName))
                throw new ArgumentException($"Entity {typeName} already exists");

            var entity = (XEntity)Activator.CreateInstance(type);
            AddEntity(entity);

            return entity;
        }

        /// <summary>
        /// create new entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cid"></param>
        /// <param name="pid"></param>
        /// <param name="propId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public T CreateEntity<T>() where T : XEntity, new()
        {
            string typeName = typeof(T).Name;
            if (entitys.ContainsKey(typeName))
                throw new ArgumentException($"Entity {typeName} already exists");

            T entity = new();
            AddEntity(entity);

            return entity;
        }

        /// <summary>
        /// add entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool AddEntity(XEntity entity)
        {
            string typeName = entity.GetType().Name;
            if (entitys.ContainsKey(typeName))
                throw new ArgumentException(typeName);

            entitys.Add(typeName, entity);
            
            var attrs = entity.GetType().GetCustomAttributes(typeof(Initialize), true);
            foreach (var attr in attrs)
            {
                if (attr is Initialize ci)
                {
                    if (ci.EveryFrameUpdate)
                        frameEntitys.Add(entity);
                }
            }

#if UNITY_EDITOR
            Debug.Log($"[{Time.frameCount}] Create entity type {typeName} iid {entity.iid}");
            XEntityEditorUtility.Notify(entity);
#endif
            return true;
        }

        /// <summary>
        /// remove entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveEntity(XEntity entity)
        {
            string typeName = entity.GetType().Name;
            if (entitys.ContainsKey(typeName))
            {
#if UNITY_EDITOR
                Debug.Log($"[{Time.frameCount}] Create entity type {typeName} iid {entity.iid}");
                XEntityEditorUtility.Notify(entity);
#endif
               
                var attrs = entity.GetType().GetCustomAttributes(typeof(Initialize), true);
                foreach (var attr in attrs)
                {
                    if (attr is Initialize ci)
                    {
                        if (ci.EveryFrameUpdate)
                            frameEntitys.Remove(entity);
                    }
                }
                
                entitys.Remove(typeName);
            }
        }

        /// <summary>
        /// query entitys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T QueryEntity<T>() where T : XEntity
        {
            string typeName = typeof(T).Name;
            if (!entitys.TryGetValue(typeName, out var entity))
                throw new NullReferenceException($"Entity {typeName} does not exist");

            return (T)entity;
        }

        /// <summary>
        /// try query entity
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool TryGetEntity(string typeName, out XEntity entity)
        {
            return entitys.TryGetValue(typeName, out entity);
        }

        /// <summary>
        /// destroy entitys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cid"></param>
        public void DestroyEntity<T>() where T : XEntity
        {
            string typeName = typeof(T).Name;
            if (entitys.TryGetValue(typeName, out var entity))
            {
                var attrs = entity.GetType().GetCustomAttributes(typeof(Initialize), true);
                foreach (var attr in attrs)
                {
                    if (attr is Initialize ci)
                    {
                        if (ci.EveryFrameUpdate)
                            frameEntitys.Remove(entity);
                    }
                }
                
                // destroy entity, remove entity from entitys, notify editor
                entity.Destroy();

#if UNITY_EDITOR
                Debug.Log($"[{Time.frameCount}] Destroy entity type {typeName} iid {entity.iid}");
                XEntityEditorUtility.Notify(entity);
#endif
                entitys.Remove(typeName);
            }
        }

        /// <summary>
        /// get all entitys
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, XEntity> GetEntitys()
        {
            return entitys;
        }

        /// <summary>
        /// update entity dispatcher, fire all entity in queue, notify all listener
        /// </summary>
        /// <param name="elapsed"></param>
        public void Update(float elapsed)
        {
            if (frameEntitys.Count > 0)
            {
                foreach (var entity in frameEntitys)
                {
                    entity.Update(elapsed);
                }
            }
        }
    }
}

