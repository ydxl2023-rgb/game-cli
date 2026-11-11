using System;
using UnityEngine;

namespace Oathx.Widget
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigAttribute : Attribute
    {
        /// <summary>
        /// path of the prefab to be bound
        /// </summary>
        public string path;

        /// <summary>
        /// at which layer is it finally attach
        /// </summary>
        public WidgetLayer layer;

        /// <summary>
        /// using asynchronous loading
        /// </summary>
        public bool async;

        /// <summary>
        /// destroy on closing
        /// </summary>
        public bool destroy;

        /// <summary>
        /// open cache
        /// </summary>
        public bool cache;

        /// <summary>
        /// canvas render order
        /// </summary>
        public int order;

        /// <summary>
        /// Display after activation
        /// </summary>
        public bool show;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="prefabPath"></param>
        /// <param name="widgetLayer"></param>
        /// <param name="asyncLoad"></param>
        public ConfigAttribute(string prefabPath, WidgetLayer widgetLayer, int sortOrder=0, bool asyncLoad=true, bool cacheWidget=true, bool activeShow=true)
        {
            path = prefabPath;
            layer = widgetLayer;
            async = asyncLoad;
            cache = cacheWidget;
            order = sortOrder;
            show = activeShow;
        }

#if UNITY_EDITOR
        public long createAt;
        public long loadTime;
#endif
    }

    public enum WidgetLayer
    {
        background,
        center,
        dialog,
        mask,
        topmost,
    }

    public abstract class IWidget : MonoBehaviour
    {
        public ConfigAttribute setting
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract bool Active();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsActived()
        {
#if UNITY_EDITOR
            if (!gameObject)
                throw new NullReferenceException();
#endif

            return gameObject.activeSelf;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void Deactive();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complete"></param>
        public virtual void Close(Action complete)
        {
            complete?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public abstract bool Ref(VM vm);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract VM GetVM();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Type GetVMType();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        public abstract void Unref(VM vm);
    }
}
