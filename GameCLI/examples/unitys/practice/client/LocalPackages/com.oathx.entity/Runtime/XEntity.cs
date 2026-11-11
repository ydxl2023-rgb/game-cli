using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace Oathx.Entity
{
    /// <summary>
    /// AES JSON Encryption Helper
    /// </summary>
    public static class AESEncryption
    {
        // default key must be 32 characters for AES-256
        private static readonly string DefaultKey = "my-32-character-secret-key!!";
        private static readonly string DefaultIV = "16-character-IV!!";

        /// <summary>
        /// AES Encryption
        /// </summary>
        /// <param name="json"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string EncryptAES(string json, string key = null, string iv = null)
        {
            if (string.IsNullOrEmpty(key))
                key = DefaultKey;
            if (string.IsNullOrEmpty(iv))
                iv = DefaultIV;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = Encoding.UTF8.GetBytes(iv.PadRight(16).Substring(0, 16));

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                        cs.Write(jsonBytes, 0, jsonBytes.Length);
                        cs.FlushFinalBlock();

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// AES Decryption
        /// </summary>
        /// <param name="encrypted"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DecryptAES(string encrypted, string key = null, string iv = null)
        {
            if (string.IsNullOrEmpty(key))
                key = DefaultKey;

            if (string.IsNullOrEmpty(iv)) 
                iv = DefaultIV;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = Encoding.UTF8.GetBytes(iv.PadRight(16).Substring(0, 16));

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                byte[] encryptedBytes = Convert.FromBase64String(encrypted);
                using (MemoryStream ms = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate Random AES Key and IV
        /// </summary>
        /// <returns></returns>
        public static (string key, string iv) GenerateRandomKeys()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                return (
                    Convert.ToBase64String(aes.Key),
                    Convert.ToBase64String(aes.IV)
                );
            }
        }
    }

    /// <summary>
    /// entity component interface, used to define the basic lifecycle and notify method for entity components, such as render component, animation component, etc.
    /// </summary>
    public interface IEntityComponent
    {
        /// <summary>
        /// awake the component, can be used to initialize component state, 
        /// such as create game object, start timer, etc., when add component to entity, etc.
        /// </summary>
        /// <returns></returns>
        bool Awake();

        /// <summary>
        /// update the component, can be used to update component state,
        /// such as animation, timer, etc., when update entity, etc.
        /// </summary>
        /// <param name="elapsed"></param>
        void Update(float elapsed);

        /// <summary>
        /// destroy the component, can be used to release resources, such as destroy game object, stop timer, etc.
        /// when remove component from entity or destroy entity, etc.
        /// </summary>
        void Destroy();

        /// <summary>
        /// invalidate the component
        /// </summary>
        void Invalidate();

        /// <summary>
        /// set entity reference, can be used to set entity reference when add component to entity,
        /// </summary>
        /// <param name="entity"></param>
        void SetEntity(XEntity entity);

        /// <summary>
        /// get entity reference, can be used to access entity properties and methods, such as notify entity change, get child entities, etc.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="eventId"></param>
        /// <param name="old"></param>
        /// <param name="value"></param>
        void Subscribe<V>(int eventId, FiledValueChangedHandler<V> callback);

        /// <summary>
        /// notify field value change
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="fieldId"></param>
        /// <param name="old"></param>
        /// <param name="value"></param>
        void FireEvent<V>(int fieldId, V old, V value);

        /// <summary>
        /// unsubscribe field value change event, can be used to remove field value change handlers
        /// </summary>
        /// <param name="fieldId"></param>
        void Unsubscribe(int fieldId);

        /// <summary>
        /// unsubscribe all field value change events, can be used to remove all field value change handlers, such as when component destroy, etc.
        /// </summary>
        void UnsubscribeAll();
    }

    /// <summary>
    /// field value changed handler, used to define the callback method for field value change event, such as property change
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <param name="fieldId"></param>
    /// <param name="old"></param>
    /// <param name="value"></param>
    public delegate void FiledValueChangedHandler<V>(int fieldId, V old, V value);

    /// <summary>
    /// entity component base class
    /// </summary>
    public abstract class XEntityComponent<T> : IEntityComponent where T : XEntity
    {
        /// <summary>
        /// field value changed handlers, used to store the field value changed handlers, such as property change handlers
        /// </summary>
        protected readonly Dictionary<int, List<Delegate>> fieldValueChangedHandlers = new Dictionary<int, List<Delegate>>();

        /// <summary>
        /// entity reference, can be used to access entity properties and methods, such as notify entity change, get child entities, etc.
        /// </summary>
        public T entity 
        { get; protected set; }

        /// <summary>
        /// set entity reference, can be used to set entity reference when add component to entity, 
        /// can be used to access entity properties and methods, such as notify entity change, get child entities, etc.
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetEntity(XEntity target)
        {
            entity = target as T;
        }

        /// <summary>
        /// get entity reference, can be used to access entity properties and methods,
        /// such as notify entity change, get child entities, etc.
        /// </summary>
        /// <returns></returns>
        public T GetEntity()
        {
            return entity;
        }
        
        /// <summary>
        /// awake the component
        /// </summary>
        public virtual bool Awake() 
        { return true; }
        
        /// <summary>
        /// update the component, can be used to update component state, such as animation, timer, etc.
        /// </summary>
        public virtual void Update(float elapsed) 
        {}

        /// <summary>
        /// destroy the component
        /// </summary>
        public virtual void Destroy() 
        {}

        /// <summary>
        /// notify component change, such as property change, can be used to trigger UI update or other logic,
        /// </summary>
        public virtual void Invalidate()
        {}

        
        /// subscribe field value change event, can be used to trigger UI update or other logic when field value change, such as property change, etc.
        public virtual void Subscribe<V>(int fieldId, FiledValueChangedHandler<V> callback)
        {
            bool exsit = fieldValueChangedHandlers.TryGetValue(fieldId, out var handler);
            if (!exsit)
            {
                handler = new List<Delegate>();
                fieldValueChangedHandlers.Add(fieldId, handler);
            }

            handler.Add(callback);
        }

        /// <summary>
        /// notify field value change, can be used to trigger UI update or other logic when field value change, such as property change, etc.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="fieldId"></param>
        /// <param name="old"></param>
        /// <param name="value"></param>
        public virtual void FireEvent<V>(int fieldId, V old, V value)
        {
            if (fieldValueChangedHandlers.TryGetValue(fieldId, out var handler))
            {
                foreach (var callback in handler)
                {
                    if (callback is FiledValueChangedHandler<V> typedCallback)
                    {
                        typedCallback.Invoke(fieldId, old, value);
                    }
                }
            }
        }

        /// <summary>
        /// unsubscribe field value change event, can be used to remove field value change handlers, such as when component destroy, etc.
        /// </summary>
        /// <param name="fieldId"></param>
        public virtual void Unsubscribe(int fieldId)
        {
            if (fieldValueChangedHandlers.ContainsKey(fieldId))
                fieldValueChangedHandlers.Remove(fieldId);
        }

        /// <summary>
        /// unsubscribe all field value change events, 
        /// can be used to remove all field value change handlers, such as when component destroy, etc.
        /// </summary>
        public virtual void UnsubscribeAll()
        {
            fieldValueChangedHandlers.Clear();
        }

        /// <summary>
        /// operator overload to check if component exists
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(XEntityComponent<T> exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// notify attribute, used to mark properties that need to notify entity change when modified, can be used to trigger UI update or other logic
    /// </summary>
    public class NotifyAttribute : Attribute
    {
    }

    /// <summary>
    /// require entity component attribute, used to mark entity components that need to be added when add the component to entity, 
    /// can be used to ensure the component dependencies,
    /// </summary>
    public class RequireEntityComponentAttribute : Attribute
    {
        /// <summary>
        /// required component types, used to mark entity components that need to be added when add the component to entity,
        /// can be used to ensure the component dependencies,
        /// </summary>
        public Type[] ComponentTypes 
        { get; private set; }

        /// <summary>
        /// require entity component, used to mark entity components that need to be added when add the component to entity, 
        /// can be used to ensure the component dependencies,
        /// </summary>
        /// <param name="componentType"></param>
        public RequireEntityComponentAttribute(Type componentType)
        {
            ComponentTypes = new Type[] { componentType };
        }

        /// <summary>
        /// require entity components, used to mark entity components that need to be added when add the component to entity,
        /// can be used to ensure the component dependencies,
        /// </summary>
        /// <param name="componentTypes"></param>
        public RequireEntityComponentAttribute(Type[] componentTypes)
        {
            ComponentTypes = componentTypes;
        }
    }

    /// <summary>
    /// xentity base class
    /// </summary>
    [Serializable]
    public class XEntity : XNumerical
    {
        /// <summary>
        /// entity components
        /// </summary>
        protected readonly List<IEntityComponent> components = new List<IEntityComponent>();
        
        /// <summary>
        /// entity instance Id
        /// </summary>
        [JsonProperty]
        public int iid
        { get; set; }

        /// <summary>
        /// entity dirty flag, used to indicate whether the entity has changed and needs to be updated
        /// can be set to true when property change, child entity change, etc., and set to false after update, etc.
        /// </summary>
        public bool dirty
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public XEntity()
        {
            iid = HashString(GetType().Name.ToLower());

            Require();
        }

        /// <summary>
        /// entity constructor
        /// </summary>
        public XEntity(int uid)
        {
            iid = uid == 0 ? HashString(GetType().Name.ToLower()) : uid;

            Require();
        }

        /// <summary>
        /// require entity components, used to add required components when add the component to entity, can be used to ensure the component dependencies,
        /// </summary>
        public void Require()
        {
            var attr = GetType().GetCustomAttribute<RequireEntityComponentAttribute>();
            if (attr != null)
            {
                foreach(var type in attr.ComponentTypes)
                {
                    AddComponent(type);
                }
            }
        }

        /// <summary>
        /// add render component
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IEntityComponent AddComponent(Type type)
        {
            var component = Activator.CreateInstance(type) as IEntityComponent;
            component.SetEntity(this);

            if (!component.Awake())
                throw new NullReferenceException(type.Name);

            components.Add(component);
            return component;
        }

        /// <summary>
        /// add render component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public virtual T AddComponent<T>() where T : XEntityComponent<XEntity>
        {
            var component = Activator.CreateInstance(typeof(T)) as T;
            component.SetEntity(this);

            if (!component.Awake())
                throw new NullReferenceException(typeof(T).Name);

            components.Add(component);
            
            return component;
        }

        /// <summary>
        /// get render component
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public virtual IEntityComponent GetComponent(Type type)
        {
            foreach (var component in components)
            {
                if (component.GetType() == type)
                    return component;
            }

            throw new NullReferenceException(type.FullName);
        }

        /// <summary>
        /// get render component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public virtual IEntityComponent GetComponent<T>() where T : IEntityComponent
        {
            foreach(var component in components)
            {
                if (component is T)
                    return component;
            }

            throw new NullReferenceException(typeof(T).FullName);
        }

        /// <summary>
        /// remove render component
        /// </summary>
        /// <param name="type"></param>
        public virtual void RemoveComponent(Type type)
        {
            foreach(var component in components)
            {
                if (component.Equals(type))
                {
                    component.UnsubscribeAll();
                    component.Destroy();
#if UNITY_EDITOR
                    UnityEngine.Debug.Log($"[{Time.frameCount}] Remove component {type.Name} from entity iid {iid}");
#endif
                    components.Remove(component);
                    break;
                }
            }
        }

        /// <summary>
        /// remove render component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public virtual void RemoveComponent<T>() where T : IEntityComponent
        {
            foreach (var component in components)
            {
                if (component is T)
                {
                    component.UnsubscribeAll();
                    component.Destroy();
#if UNITY_EDITOR
                    UnityEngine.Debug.Log($"[{Time.frameCount}] Remove component {typeof(T).Name} from entity iid {iid}");
#endif
                    components.Remove(component);
                    break;
                }
            }
        }

        /// <summary>
        /// remove all render components
        /// </summary>
        public virtual void RemoveAllComponents()
        {
            foreach (var component in components)
            {
                component.UnsubscribeAll();
                component.Destroy();
            }

            components.Clear();
        }

        /// <summary>
        /// notify field value change, can be used to trigger UI update or other logic when field value change, such as property change, etc.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="Id"></param>
        /// <param name="old"></param>
        /// <param name="value"></param>
        public void NotifyComponents<V>(int Id, V old, V value)
        {
            foreach(var compoment in components)
            {
                compoment.FireEvent(Id, old, value);
            }
        }

        /// <summary>
        /// update the entity, can be used to update entity state, such as animation, timer, etc.
        /// </summary>
        /// <param name="elapsed"></param>
        public virtual void Update(float elapsed)
        {
            foreach (var comp in components) 
            {
                comp.Update(elapsed);
            }
        }

        /// <summary>
        /// destroy the entity
        /// </summary>
        public virtual void Destroy()
        {
            RemoveAllComponents();
        }

        /// <summary>
        /// hash string to int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual int HashString(string str)
        {
            const uint InitialFNV = 2166136261U;
            const uint FNVMultiple = 16777619;

            uint hash = InitialFNV;
            for (int i = 0; i < str.Length; i++)
            {
                hash ^= str[i];
                hash *= FNVMultiple;
            }

            return (int)(hash & 0x7FFFFFFF);
        }

        /// <summary>
        /// operator overload to check if entity exists
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(XEntity exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// xentity serialize class
    /// </summary>
    public class XEntitySerialize : XEntity
    {
          /// <summary>
        /// entity timestamp
        /// </summary>
        [JsonProperty]
        public string timestamp
        { get; protected set; }

        /// <summary>
        /// entity version, used for compatibility check when deserialize, can be set to any string, such as "1.0", "2024-01-01", etc.
        /// </summary>
        [JsonProperty]
        public string version
        { get; set; }

        /// <summary>
        /// entity valid flag
        /// </summary>
        [JsonIgnore]
        public bool valid
        { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public XEntitySerialize()
            : base()
        {
            // force malloc timestamp
            timestamp = DateTime.UtcNow.ToString();
        }

        /// <summary>
        /// entity constructor
        /// </summary>
        public XEntitySerialize(int uid)
            : base(uid)
        {
            // force malloc timestamp
            timestamp = DateTime.UtcNow.ToString();
        }

        /// <summary>
        /// get file path
        /// </summary>
        /// <returns></returns>
        public virtual string GetSerializeFilePath()
        {
            return Path.Combine(Application.persistentDataPath,
                HashString(GetType().Name.ToLower()).ToString()).Replace("\\", "/");
        }

        /// <summary>
        /// check if serialize file exists
        /// </summary>
        /// <returns></returns>
        public virtual bool ExistsSerializeFile()
        {
            var path = GetSerializeFilePath();
            return File.Exists(path) || File.Exists(path + ".bak");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid() { return valid; }

        /// <summary>
        /// Serialize the entity
        /// </summary>
        /// <returns></returns>
        public virtual string Serialize()
        {
            // force update timestamp
            timestamp = DateTime.UtcNow.ToString();

            try
            {
#if UNITY_EDITOR
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // serialize to json
                string text = JsonConvert.SerializeObject(this);
                if (string.IsNullOrEmpty(text))
                    throw new NullReferenceException($"{GetType().Name} serialize failure");

                var path = GetSerializeFilePath();
#if USE_AES_ENCRYPTION
                var encryptedText = AESEncryption.EncryptAES(text);
                WriteAllTextAtomic(path, encryptedText);
#if UNITY_EDITOR
                stopwatch.Stop();
                UnityEngine.Debug.Log($"{GetType().Name}.Serialize {path} elapsed {stopwatch.ElapsedMilliseconds}");
#endif
                return encryptedText;
#else
                WriteAllTextAtomic(path, text);
                return text;
#endif
            } 
            catch(Exception exception)
            {
                UnityEngine.Debug.LogError(exception);
            }

            return string.Empty;
        }

        /// <summary>
        /// Deserialize the entity
        /// </summary>
        public virtual bool Deserialize()
        {
            var path = GetSerializeFilePath();
            bool mainExists = File.Exists(path);
            if (mainExists)
            {
                var text = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(text))
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.Log($"{GetType().Name}.Deserialize {path}");
#endif
                    if (Deserialize(text))
                    {
                        return true;
                    }
                }
            }

            // 主存档缺失或损坏：回退到原子替换留下的 .bak 备份
            var backupPath = path + ".bak";
            if (File.Exists(backupPath))
            {
                var backupText = File.ReadAllText(backupPath);
                if (!string.IsNullOrEmpty(backupText) && Deserialize(backupText))
                {
                    UnityEngine.Debug.LogWarning($"{GetType().Name}.Deserialize recovered from backup {backupPath}");
                    return true;
                }
            }

            if (mainExists)
            {
                // 文件存在但主备均不可读：存档已损坏。必须报错（此前静默吞掉，导致空存档覆盖）
                UnityEngine.Debug.LogError($"{GetType().Name}.Deserialize failed: archive corrupted at {path}");
            }

            return false;
        }

        /// <summary>
        /// Deserialize the entity
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual bool Deserialize(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return false;

#if UNITY_EDITOR
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
#endif

#if USE_AES_ENCRYPTION
                text = AESEncryption.DecryptAES(text);
#endif
                var obj = JsonConvert.DeserializeObject(text, GetType());
                MergeUtility.Merge(this, obj);

                // assume valid unless exception occurs
                valid = true;

#if UNITY_EDITOR
                stopwatch.Stop();
                UnityEngine.Debug.Log($"{GetType().Name}.Deserialize elapsed {stopwatch.ElapsedMilliseconds}");

                XEntityEditorUtility.Notify(this);
#endif
            }
            catch (Exception exception) 
            {
                valid = false;
                // 真机也必须可见：此前仅在编辑器打日志，设备上存档损坏完全静默
                UnityEngine.Debug.LogException(exception);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 原子写存档：先写 .tmp 再替换正式文件，替换前把旧文件保留为 .bak，
        /// 防止进程在写入中途被杀导致存档文件截断损坏。
        /// </summary>
        private static void WriteAllTextAtomic(string path, string contents)
        {
            var tmpPath = path + ".tmp";
            var backupPath = path + ".bak";

            File.WriteAllText(tmpPath, contents);

            if (!File.Exists(path))
            {
                File.Move(tmpPath, path);
                return;
            }

            try
            {
                File.Replace(tmpPath, path, backupPath);
            }
            catch (Exception)
            {
                // 平台不支持 File.Replace 时退化：先备份再覆盖
                File.Copy(path, backupPath, true);
                File.Copy(tmpPath, path, true);
                File.Delete(tmpPath);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class MergeUtility
    {
        /// <summary>
        /// Merge source object into target object, only merge properties and fields that exist in both objects
        /// and only merge non-default values from source to target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static bool Merge(object target, object source)
        {
            if (target == null || source == null) 
                return false;

            return MergeRecursive(target, source, target.GetType());
        }

        /// <summary>
        /// Merge source object into target object recursively, only merge properties and fields that exist in both objects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        private static bool MergeRecursive(object target, object source, Type type)
        {
            if (target == null || source == null)
                return false;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsNotSerialized || field.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                object targetValue = field.GetValue(target);
                object sourceValue = field.GetValue(source);

                object mergedValue = MergeValue(targetValue, sourceValue, field.FieldType);
                if (!ReferenceEquals(mergedValue, targetValue))
                    field.SetValue(target, mergedValue);
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (!prop.CanWrite || !prop.CanRead || prop.GetIndexParameters().Length > 0)
                    continue;

                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                object targetValue = prop.GetValue(target);
                object sourceValue = prop.GetValue(source);
                object mergedValue = MergeValue(targetValue, sourceValue, prop.PropertyType);

                if (!ReferenceEquals(mergedValue, targetValue))
                    prop.SetValue(target, mergedValue);
            }

            return true;
        }

        /// <summary>
        /// Merge source value into target value, only merge non-default values from source to target,
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="sourceValue"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private static object MergeValue(object targetValue, object sourceValue, Type valueType)
        {
            if (IsSimpleType(valueType))
            {
                if (IsDefaultValue(targetValue, valueType))
                    return sourceValue;

                return targetValue;
            }

            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return MergeList(targetValue as IList, sourceValue as IList, valueType);
            }

            if (valueType.IsArray)
            {
                return MergeArray(targetValue as Array, sourceValue as Array, valueType);
            }

            if (valueType.IsClass && valueType != typeof(string))
            {
                if (targetValue == null)
                    return sourceValue;
             
                if (sourceValue == null)
                    return targetValue;
                
                MergeRecursive(targetValue, sourceValue, valueType);
                return targetValue;
            }

            return sourceValue;
        }

        /// <summary>
        /// Merge source list into target list, only merge non-default values from source to target, and only merge elements at the same index,
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="sourceList"></param>
        /// <param name="listType"></param>
        /// <returns></returns>
        private static IList MergeList(IList targetList, IList sourceList, Type listType)
        {
            if (sourceList == null || sourceList.Count == 0)
                return targetList ?? (IList)Activator.CreateInstance(listType);

            if (targetList == null)
                targetList = (IList)Activator.CreateInstance(listType);

            Type elementType = listType.GetGenericArguments()[0];
            int targetCount = targetList.Count;
            int sourceCount = sourceList.Count;

            for (int i = 0; i < Math.Min(targetCount, sourceCount); i++)
            {
                object merged = MergeValue(targetList[i], sourceList[i], elementType);
                if (!ReferenceEquals(merged, targetList[i]))
                    targetList[i] = merged;
            }

            if (sourceCount > targetCount)
            {
                for (int i = targetCount; i < sourceCount; i++)
                {
                    targetList.Add(sourceList[i]);
                }
            }
            
            return targetList;
        }

        /// <summary>
        /// Merge source array into target array, only merge non-default values from source to target, and only merge elements at the same index,
        /// </summary>
        /// <param name="targetArray"></param>
        /// <param name="sourceArray"></param>
        /// <param name="arrayType"></param>
        /// <returns></returns>
        private static Array MergeArray(Array targetArray, Array sourceArray, Type arrayType)
        {
            if (sourceArray == null || sourceArray.Length == 0)
                return targetArray ?? Array.CreateInstance(arrayType.GetElementType(), 0);

            Type elementType = arrayType.GetElementType();
            int targetLen = targetArray?.Length ?? 0;
            int sourceLen = sourceArray.Length;
            int newLen = Math.Max(targetLen, sourceLen);
            Array newArray = Array.CreateInstance(elementType, newLen);

            for (int i = 0; i < targetLen; i++)
            {
                if (i < sourceLen)
                {
                    object merged = MergeValue(targetArray.GetValue(i), sourceArray.GetValue(i), elementType);
                    newArray.SetValue(merged, i);
                }
                else
                {
                    newArray.SetValue(targetArray.GetValue(i), i);
                }
            }

            for (int i = targetLen; i < sourceLen; i++)
            {
                newArray.SetValue(sourceArray.GetValue(i), i);
            }
            
            return newArray;
        }

        /// <summary>
        /// check if type is simple type, such as primitive type, string, decimal, DateTime, DateTimeOffset, Guid, Uri, Enum, etc., or nullable of simple type,
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSimpleType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(Guid) ||
                   type == typeof(Uri) ||
                   type.IsEnum;
        }

        /// <summary>
        /// check if value is default value of the type, such as 0 for int, 0.0 for float, false for bool, null for reference type, etc., or nullable of simple type,
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsDefaultValue(object value, Type type)
        {
            if (value == null) return true;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.IsValueType)
            {
                object defaultValue = Activator.CreateInstance(type);
                return value.Equals(defaultValue);
            }
            return false;
        }
    }
}
