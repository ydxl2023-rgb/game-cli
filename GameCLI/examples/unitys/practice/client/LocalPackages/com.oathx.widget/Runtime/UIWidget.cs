using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Widget
{
#pragma warning disable IDE1006
    public class Property
    {
        public ISyncAction action
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Component> components
        { get; private set; }

        /// <summary>
        /// Property
        /// </summary>
        /// <param name="syncAction"></param>
        /// <param name="syncComponents"></param>
        public Property(ISyncAction syncAction, List<Component> syncComponents)
        {
            action = syncAction;
            components = syncComponents;
        }
    }


    /// <summary>
    /// base widget
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UIWidget<T> : IWidget where T : VM
    {
        private readonly List<Property> propertys = new List<Property>();
       
        /// <summary>
        /// View Modle owned by Widgets
        /// </summary>
        public T vm
        { get; private set; }

        /// <summary>
        /// get current bind vm
        /// </summary>
        /// <returns></returns>
        public override VM GetVM()
        {
            return vm;
        }

        /// <summary>
        /// get vm type
        /// </summary>
        /// <returns></returns>
        public override System.Type GetVMType()
        {
            return typeof(T);
        }

        /// <summary>
        /// check widget is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return this != null && gameObject != null && gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        public override bool Ref(VM refVM)
        {
            var metadata = gameObject.GetComponent<Metadata>();
            if (!metadata)
                throw new System.NullReferenceException($"{typeof(T).Name} missing component {typeof(Metadata).Name}");

            var count = metadata.metadatas.Count;
            if (count <= 0)
                return false;

            for(int idx=0; idx<count; idx++)
            {
                var metadataField = metadata.metadatas[idx];
                if (metadataField != null)
                {
                    var action = MetadataSyncFactory.GetSingleton().GetAction(metadataField.type);
                    propertys.Add(new Property(action, metadataField.components));
                }
            }

            if (refVM is T v)
            {
                vm = v;

                // register property changed
                vm.PropertyChanged += OnPropertyChanaged;
                vm.DelayInitialize();
            }
            else
            {
                Debug.LogError(typeof(T).Name);
            }
               
            return true;
        }

        /// <summary>
        /// vm property changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyIndex"></param>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        protected virtual void OnPropertyChanaged(object sender, int propertyIndex, string propertyName, object newValue)
        {
            if (propertyIndex >= 0 && propertyIndex < propertys.Count)
            {
                Property property = propertys[propertyIndex];
                if (property != null)
                {
                    foreach (var component in property.components)
                    {
                        property.action.Sync(propertyName, newValue, property, component);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        public override void Unref(VM refVM)
        {
            vm.PropertyChanged -= OnPropertyChanaged;

            foreach (var property in propertys)
            {
                if (property.action != null)
                    property.action.UnSync(property);
            }
        }

        /// <summary>
        /// reset entity data
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <param name="entity"></param>
        public virtual void Invalidate<Entity>(Entity entity)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Active()
        {
            if (gameObject && !gameObject.activeSelf)
                gameObject.SetActive(true);
#if UNITY_EDITOR
            Debug.Log($"{name} Active succeed");
#endif
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Deactive()
        {
            if (gameObject && gameObject.activeSelf)
                gameObject.SetActive(false);

#if UNITY_EDITOR
            Debug.Log($"{name} Deactive succeed");
#endif
        }
    }

    /// <summary>
    /// widget property changed notify sync data
    /// </summary>
    /// <param name="vm"></param>
    /// <param name="index"></param>
    /// <param name="name"></param>
    /// <param name="newValue"></param>
    public delegate void WidgetPropertyChangedEvent(object vm, int index, string name, object newValue);

    /// <summary>
    /// widget property changed
    /// </summary>
    interface IWidgetPropertyChanged
    {
        public event WidgetPropertyChangedEvent PropertyChanged;
    }

    /// <summary>
    /// value box
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Boxed<T>
    {
        public T value;

        /// <summary>
        /// construct function
        /// </summary>
        /// <param name="v"></param>
        public Boxed(T v)
        {
            value = v;
        }

        /// <summary>
        /// convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// View modle base class
    /// </summary>
    public class VM : IWidgetPropertyChanged
    {
        public event WidgetPropertyChangedEvent PropertyChanged;

        /// <summary>
        /// cache property value
        /// </summary>
        protected class VCache
        {
            /// <summary>
            /// property index
            /// </summary>
            public int index;

            /// <summary>
            /// property value
            /// </summary>
            public object value;

            /// <summary>
            /// property name
            /// </summary>
            public string name;
        }

        /// <summary>
        /// delay invokes
        /// </summary>
        protected VCache[] delayInvokes;

        /// <summary>
        /// cache notify fields
        /// </summary>
        protected object[] notifyFields;

        /// <summary>
        /// VM object init, create field array, inject to construct call
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual void Init(int count)
        {
            if (count > 0)
            {
                delayInvokes = new VCache[count];
                notifyFields = new object[count];
            }
        }

        /// <summary>
        /// get field info
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual T Get<T>(int index)
        {
            var value = notifyFields[index];
            if (value != null)
            {
                if (value is Boxed<T> boxed)
                {
                    return boxed.value;
                }
                else
                {
                    return (T)value;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="propertyName"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        protected virtual bool Modify<T>(T value, int index, string propertyName, object oldValue)
        {
            if (typeof(T).IsValueType)
            {
                var newValue = new Boxed<T>(value);
                notifyFields[index] = newValue;

                if (PropertyChanged == null)
                    delayInvokes[index] = new VCache() { index = index, value = newValue, name = propertyName };

                PropertyChanged?.Invoke(this, index, propertyName, newValue);
                return true;
            }
            else
            {
                notifyFields[index] = value;
                if (PropertyChanged == null)
                    delayInvokes[index] = new VCache() { index = index, value = value, name = propertyName };

                PropertyChanged?.Invoke(this, index, propertyName, value);
                return true;
            }
        }

        /// <summary>
        /// set value, inject to property setter call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual bool Set<T>(T value, int index, string propertyName)
        {
            var oldValue = notifyFields[index];
            if (oldValue == null)
            {
                return Modify(value, index, propertyName, oldValue);
            }
            else
            {
                if (oldValue is Boxed<T> newValue)
                {
                    if (!EqualityComparer<T>.Default.Equals(newValue.value, value))
                    {
                        newValue.value = value;
                        if (PropertyChanged == null)
                            delayInvokes[index] = new VCache() { index = index, value = newValue, name = propertyName };

                        PropertyChanged?.Invoke(this, index, propertyName, newValue);
                    }
                }
                else
                {
                    notifyFields[index] = value;
                    if (PropertyChanged == null)
                        delayInvokes[index] = new VCache() { index = index, value = value, name = propertyName };

                    PropertyChanged?.Invoke(this, index, propertyName, value);
                }
            }

            return true;
        }

        /// <summary>
        /// delay initialize, notify property changed
        /// </summary>
        public virtual void DelayInitialize()
        {
            if (delayInvokes != null)
            {
                for (int i = 0; i < delayInvokes.Length; i++)
                {
                    if (delayInvokes[i] != null && delayInvokes[i] is VCache v)
                    {
                        PropertyChanged?.Invoke(this, v.index, v.name, v.value);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Reset()
        {
            PropertyChanged = null;

            for (int i = 0; i < notifyFields.Length; i++)
            {
                delayInvokes[i] = null;
                notifyFields[i] = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        public static implicit operator bool(VM vm)
        {
            return vm != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BindingAttribute : System.Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string name
        {  get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool SkipEqualsCheck
        { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public BindingAttribute()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public BindingAttribute(System.Type type)
        {
            name = type.Name;
        }
    }
#pragma warning restore IDE1006
}
