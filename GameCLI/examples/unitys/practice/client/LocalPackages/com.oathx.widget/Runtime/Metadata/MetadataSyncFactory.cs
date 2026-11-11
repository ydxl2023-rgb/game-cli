using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Oathx.Widget
{
    public interface ISyncAction
    {
        void Sync(string propertyName, object value, Property property, Component component);
        void UnSync(Property property);
    }

    /// <summary>
    /// 
    /// </summary>
    [Binding(typeof(RectTransform))]
    public class SyncVisible : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            var targetGo = component.gameObject;
            if (!targetGo)
                throw new System.NullReferenceException(propertyName);

            if (value is Boxed<bool> c)
            {
                targetGo.SetActive(c.value);
            }
            else
            {
                throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<bool>)}");
            }
        }

        public void UnSync(Property property)
        {

        }
    }

    [Binding(typeof(Image))]
    public class SyncImage : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Image c)
            {
                if (value is string path)
                {
                    UIResourceManager.GetSingleton().LoadAsync<Sprite>(path, (sprite) => {
                        c.sprite = (Sprite)sprite;
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(string)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Image] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is Image c)
                {
                    UIResourceManager.GetSingleton().Unload(c.sprite);
                }
            }
        }
    }

    [Binding(typeof(Image))]
    public class SyncImageAmount : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Image c)
            {
                if (value is Boxed<float> amount)
                {
                    c.fillAmount = amount.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(string)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"Bind type error " +
                    $"{typeof(SyncImageAmount).Name} sync only support Image");
            }
        }

        public void UnSync(Property property)
        {
           
        }
    }

    [Binding(typeof(Text))]
    public class SyncText : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Text c)
            {
                c.text = value.ToString();
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Text] do not match");
            }
        }

        public void UnSync(Property property)
        {
            
        }
    }

#if !USE_OLD_TEXT
    [Binding(typeof(TMPro.TextMeshProUGUI))]
    public class SyncTextMeshPro : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is TMPro.TextMeshProUGUI c)
            {
                c.text = value.ToString();
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [TextMeshProUGUI] do not match");
            }
        }

        public void UnSync(Property property)
        {

        }
    }
#endif

    
    [Binding(typeof(CanvasGroup))]
    public class SyncAlpha : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is CanvasGroup c)
            {
                if (value is Boxed<float> v)
                    c.alpha = v.value;
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<float>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [CanvasGroup] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(Graphic))]
    public class SyncColor : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Graphic c)
            {
                if (value is Boxed<Color> v)
                {
                    c.color = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<UnityEngine.Color>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Graphic] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(RawImage))]
    public class SyncRawImage : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RawImage c)
            {
                if (value is string path)
                {
                    UIResourceManager.GetSingleton().LoadAsync<Texture>(path, (v) => {
                        c.texture = (Texture)v;
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(string)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RawImage] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is RawImage c)
                {
                    UIResourceManager.GetSingleton().Unload(c.texture);
                }
            }
        }
    }

    [Binding(typeof(RawImage))]
    public class SyncTexture : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RawImage c)
            {
                if (value is Texture texture)
                {
                    c.texture = texture;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Texture)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [SyncTexture] do not match");
            }
        }

        public void UnSync(Property property)
        {
  
        }
    }

    [Binding(typeof(Animator))]
    public class SyncAnimator : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Animator c)
            {
                if (value is string clip)
                {
                    c.enabled = true;

                    // force play animation clip
                    c.Play(clip);
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Texture)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Animator] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }


    [Binding(typeof(Image))]
    public class SyncSprite : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Image c)
            {
                if (value is Sprite sprite)
                {
                    c.sprite = sprite;
                }
                else if (value is string path)
                {
                    UIResourceManager.GetSingleton().LoadAsync<Sprite>(path, (sprite) => {
                        c.sprite = sprite as Sprite;
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure" +
                        $" Invalid cast {value.GetType()} to {typeof(Sprite)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Image] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(Graphic))]
    public class SyncRaycast : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Graphic c)
            {
                if (value is Boxed<bool> v)
                {
                    c.raycastTarget = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<bool>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Graphic] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(Canvas))]
    public class SyncSortingOrder : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Canvas c)
            {
                if (value is Boxed<int> v)
                {
                    c.sortingOrder = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<int>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Canvas] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncScale : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Vector3> v)
                {
                    c.localScale = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<Vector3>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is RectTransform c)
                {
                    c.localScale = Vector3.one;
                }
            }
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncSizeDelta : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Vector2> v)
                {

                    c.sizeDelta = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<Vector2>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncAnchoredPosition : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Vector2> v)
                {
                    c.anchoredPosition = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<Vector2>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncQuaternion : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Quaternion> v)
                {
                    c.rotation = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<UnityEngine.Quaternion>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncEulerAngles : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Vector3> v)
                {
                    c.eulerAngles = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<Vector3>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncPosition : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Vector3> v)
                {
                    c.position = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<Vector3>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }


    [Binding(typeof(RectTransform))]
    public class SyncLocalPosition : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is Boxed<Vector3> v)
                {
                    c.localPosition = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<Vector3>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [RectTransform] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(Text))]
    public class SyncDate : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Text c)
            {
                if (value is Boxed<long> v)
                {
                    System.DateTime dt = new System.DateTime(v.value);
                    c.text = dt.ToShortTimeString();
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<long>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Text] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(Slider))]
    public class SyncSlider : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Slider c)
            {
                if (value is Boxed<float> v)
                {
                    c.value = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<float>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Slider] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    public delegate bool SliderChange(Slider sender, float value);

    [Binding(typeof(Slider))]
    public class SyncSliderChange : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Slider c)
            {
                if (value is SliderChange e)
                {
                    c.onValueChanged.AddListener((v) => {
                        e.Invoke(c, v);
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure" +
                        $" Invalid cast {value.GetType()} to {typeof(SliderChange).Name}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Slider] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is Slider c)
                {
                    c.onValueChanged.RemoveAllListeners();
                }
            }
        }
    }

    [Binding(typeof(Toggle))]
    public class SyncToggle : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Toggle c)
            {
                if (value is Boxed<bool> v)
                {
                    c.isOn = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure" +
                        $" Invalid cast {value.GetType()} to {typeof(Boxed<bool>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Toggle] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    public delegate bool ToggleChange(Toggle sender, bool value);
    
    [Binding(typeof(Toggle))]
    public class SyncToggleChange : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Toggle c)
            {
                if (value is ToggleChange evt)
                {
                    c.onValueChanged.AddListener((v) => {
                        evt?.Invoke(c, v);
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure " +
                        $"Invalid cast {value.GetType()} to {typeof(ToggleChange).Name}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} " +
                    $"and synchronizer [Toggle] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is Toggle c)
                {
                    c.onValueChanged.RemoveAllListeners();
                }
            }
        }
    }

    [Binding(typeof(Metadata))]
    public class SyncMetadata : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            var widget = component.gameObject.GetComponent<IWidget>();
            if (!widget)
                throw new System.NullReferenceException($"{propertyName} missing component {typeof(IWidget).Name}");

            if (value != null)
            {
                if (value is VM vm)
                {
                    // if current widget ref VM, we must be unref the VM
                    var old = widget.GetVM();
                    if (old)
                    {
                        widget.Unref(old);
                    }
                        
                    widget.Ref(vm);
                }
            }
            else
            {
                // if current widget ref VM, we must be unref the VM
                var old = widget.GetVM();
                if (old)
                    widget.Unref(old);
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    [Binding(typeof(Scrollbar))]
    public class SyncScrollbar : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Scrollbar c)
            {
                if (value is Boxed<float> v)
                {
                    c.value = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure " +
                        $"Invalid cast {value.GetType()} to {typeof(Boxed<float>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Scrollbar] do not match");
            }
        }

        public void UnSync(Property property)
        {
        }
    }

    public delegate bool ButtonClick(Button sender);

    [Binding(typeof(Button))]
    public class SyncClick : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Button button)
            {
                if (value is ButtonClick click)
                {
                    button.onClick.AddListener(() => click?.Invoke(button));
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure " +
                        $"Invalid cast {value.GetType()} to {typeof(ButtonClick).Name}");
                }  
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Button] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is Button c)
                {
                    c.onClick.RemoveAllListeners();
                }
            }
        }
    }

    [Binding(typeof(Button))]
    public class SyncInteractable : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Button button)
            {
                if (value is Boxed<bool> v)
                {
                    button.interactable = v.value;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Boxed<bool>)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Button] do not match");
            }
        }

        public void UnSync(Property property)
        {

        }
    }

    [Binding(typeof(Material))]
    public class SyncMaterial : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is Graphic c)
            {
                if (value is Material v)
                {
                    c.material = v;
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure Invalid cast {value.GetType()} to {typeof(Material)}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Graphic] do not match");
            }
        }

        public void UnSync(Property property)
        {

        }
    }

    public enum ContainerOpCode
    {
        Added,
        Remove,
        Clear,
        Reset,
        Modify,
    }

    public delegate void ContainerChanged(IContainer sender, ContainerOpCode opCode, int index, VM element);

    /// <summary>
    /// 
    /// </summary>
    public interface IContainer : IEnumerable
    {
        event ContainerChanged containerChange;

        int Count { get;}

        /// <summary>
        /// Return the element at the specified index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        VM GetAt(int index);

        /// <summary>
        /// try convert list to Array, if failure then return false
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        bool TryToArray(out VM[] array);
    }

    /// <summary>
    /// implement vm list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VList<T> : IContainer where T : VM
    {
        public event ContainerChanged containerChange;

        /// <summary>
        /// all elements
        /// </summary>
        protected List<T> elements;

        /// <summary>
        /// 
        /// </summary>
        public VList()
        {
            elements = new List<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        public VList(int capacity)
        {
            elements = new List<T>(capacity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents"></param>
        public VList(List<T> contents)
        {
            elements = contents;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents"></param>
        public static implicit operator VList<T>(List<T> contents)
        {
            return new VList<T>(contents);
        }

        /// <summary>
        /// implement enumerator 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        /// <summary>
        /// implement base function
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VM GetAt(int index)
        {
            return elements[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return elements[index];
            }

            set
            {
                Set(index, value);
            }
        }

        /// <summary>
        /// get current container elements count
        /// </summary>
        public int Count
        {
            get { return elements.Count; }
        }

        /// <summary>
        /// force modify element object
        /// </summary>
        /// <param name="index"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool Set(int index, T element)
        {
            if (index < 0 || index >= elements.Count)
                return false;

            elements[index] = element;

            if (containerChange != null)
                containerChange.Invoke(this, ContainerOpCode.Modify, index, element);

            return true;
        }

        /// <summary>
        /// add new element
        /// </summary>
        /// <param name="element"></param>
        public void Add(T element)
        {
            elements.Add(element);

            if (containerChange != null)
                containerChange.Invoke(this, ContainerOpCode.Added, elements.Count - 1, element);
        }

        /// <summary>
        /// add new elements
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(IEnumerable<T> collection)
        {
            foreach(var element in collection)
            {
                Add(element);
            }
        }

        /// <summary>
        /// clear all elements
        /// </summary>
        public void Clear()
        {
            for(int index=0; index<elements.Count; index++)
            {
                if (containerChange != null)
                    containerChange.Invoke(this, ContainerOpCode.Remove, index, elements[index]);
            }

            elements.Clear();
        }

        /// <summary>
        /// check element 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return elements.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        public void CopyTo(T[] array)
        {
            elements.CopyTo(array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="count"></param>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            elements.CopyTo(index, array, arrayIndex, count);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool Exists(System.Predicate<T> match)
        {
            return elements.Exists(match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T Find(System.Predicate<T> match)
        {
            return elements.Find(match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> FindAll(System.Predicate<T> match)
        {
            return elements.FindAll(match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(int startIndex, int count, System.Predicate<T> match)
        {
            return elements.FindIndex(startIndex, count, match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(int startIndex, System.Predicate<T> match)
        {
            return elements.FindIndex(startIndex, match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindIndex(System.Predicate<T> match)
        {
            return elements.FindIndex(match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T FindLast(System.Predicate<T> match)
        {
            return elements.FindLast(match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(int startIndex, int count, System.Predicate<T> match)
        {
            return elements.FindLastIndex(startIndex, count, match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(int startIndex, System.Predicate<T> match)
        {
            return elements.FindLastIndex(startIndex, match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int FindLastIndex(System.Predicate<T> match)
        {
            return elements.FindLastIndex(match);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(System.Comparison<T> comparison)
        {
            elements.Sort(comparison);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="comparer"></param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            elements.Sort(index, count, comparer);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Sort()
        {
            elements.Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer)
        {
            elements.Sort(comparer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            return elements.ToArray();
        }

        public bool TryToArray(out VM[] array)
        {
            array = ToArray();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int IndexOf(T item, int index, int count)
        {
            return elements.IndexOf(item, index, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int IndexOf(T item, int index)
        {
            return elements.IndexOf(item, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public void Remove(T element)
        {
            var index = elements.IndexOf(element);
            if (index >= 0)
            {
                elements.RemoveAt(index);

                if (containerChange != null)
                    containerChange.Invoke(this, ContainerOpCode.Remove, index, element);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= elements.Count)
                return false;

            var element = elements[index];
            elements.RemoveAt(index);

            if (containerChange != null)
                containerChange.Invoke(this, ContainerOpCode.Remove, index, element);

            return true;
        }
    }

    [Binding(typeof(ScrollRect))]
    public class SyncScrollRect : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is ScrollRect target)
            {
                if (value is IContainer container)
                {
                    var prefab = FindDefaultItemGo(target);
                    
                    container.containerChange += (sender, opCode, index, element) => {
                        if (component is ScrollRect scrollView)
                        {
                            switch (opCode)
                            {
                                case ContainerOpCode.Added:
                                    Add(target, prefab, element);
                                    break;

                                case ContainerOpCode.Remove:
                                    Remove(target, index, element);
                                    break;
                            }
                        }
                    };
                        
                    for(int index=0; index<container.Count; index++)
                    {
                        Add(target, prefab, container.GetAt(index));
                    }
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [SyncScrollRect] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is IWidget c)
                {
                    var vm = c.GetVM();
                    if (vm)
                        c.Unref(vm);

                    GameObject.Destroy(c.gameObject);
                }
            }
        }

        private GameObject FindDefaultItemGo(ScrollRect scroll)
        {
            for (int index=0; index<scroll.viewport.childCount; index++)
            {
                var trans = scroll.viewport.GetChild(index);
                if (trans)
                {
                    var component = trans.GetComponent<IWidget>();
                    if (component)
                        return trans.gameObject;
                }
            }

            throw new System.NullReferenceException("The Unity ScrollRect Component lacks an interface for setting the prototype prefab of the scrolling view. " +
                "We strictly require that the Viewport must be attached to the prefab prototype to be created, and this prototype must be an IWidget");
        }

        private bool Add(ScrollRect scroll, GameObject prefab, VM element)
        {
            var itemGo = GameObject.Instantiate(prefab);
            if (!itemGo)
                throw new System.NullReferenceException();

            itemGo.gameObject.SetActive(true);
           
            itemGo.transform.SetParent(scroll.content, true);
            itemGo.transform.localScale = Vector3.one;
            itemGo.transform.position = Vector3.zero;
            itemGo.transform.eulerAngles = Vector3.zero;

            var widget = itemGo.GetComponent<IWidget>();
            if (widget)
            {
                var old = widget.GetVM();
                if (old)
                {
                    if (old != element)
                        widget.Unref(old);
                }

                widget.Ref(element);
            }

            return true;
        }

        private void Remove(ScrollRect scrollRect, int index, VM element)
        {
            if (!scrollRect.content)
                throw new System.NullReferenceException($"Can't find content");

            if (scrollRect.content.childCount > 0)
            {
                var child = scrollRect.content.GetChild(index);
                if (child)
                {
                    var widget = child.GetComponent<IWidget>();
                    if (widget)
                    {
                        widget.Unref(element);

                        // real destory the child
                        GameObject.Destroy(child.gameObject);
                    }
                }
            }
        }
    }

    [Binding(typeof(RectTransform))]
    public class SyncGroup : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is RectTransform c)
            {
                if (value is IContainer container)
                {
                    for (int index = 0; index < c.childCount; index++)
                    {
                        var child = c.GetChild(index);
                        if (child)
                        {
                            var widget = child.GetComponent<IWidget>();
                            if (!widget)
                                throw new System.NullReferenceException($"{child.gameObject.name} Missing component IWidget");

                            widget.Ref(container.GetAt(index));
                        }
                    }
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {component.gameObject} failure " +
                        $"Invalid cast {value.GetType()} to IContainer");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [Button] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is RectTransform c)
                {
                    for (int index = 0; index < c.childCount; index++)
                    {
                        var child = c.GetChild(index);
                        if (child)
                        {
                            var widget = child.GetComponent<IWidget>();
                            if (!widget)
                                throw new System.NullReferenceException($"{child.gameObject.name} Missing component IWidget");

                            var vm = widget.GetVM();
                            if (vm)
                                widget.Unref(vm);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// metadata sync factory
    /// </summary>
    public class MetadataSyncFactory
    {
        protected static readonly MetadataSyncFactory instance = new MetadataSyncFactory();

        /// <summary>
        /// metadata sync factory singleton
        /// </summary>
        /// <returns></returns>
        public static MetadataSyncFactory GetSingleton()
        {
            return instance;
        }

        /// <summary>
        /// all sync action factorys
        /// </summary>
        protected readonly Dictionary<string, ISyncAction> factorys = new Dictionary<string, ISyncAction>();

        /// <summary>
        /// 
        /// </summary>
        public MetadataSyncFactory()
        {
            System.Type[] regTypes = new System.Type[]
            {
                typeof(SyncVisible),
                typeof(SyncAlpha),
                typeof(SyncText),
                typeof(SyncImage),
                typeof(SyncImageAmount),
                typeof(SyncSprite),
                typeof(SyncRawImage),
                typeof(SyncDate),
                typeof(SyncScale),
                typeof(SyncSizeDelta),
                typeof(SyncPosition),
                typeof(SyncEulerAngles),
                typeof(SyncSlider),
                typeof(SyncToggle),
                typeof(SyncScrollbar),
                typeof(SyncMetadata),
                typeof(SyncClick),
                typeof(SyncInteractable),
                typeof(SyncRaycast),
                typeof(SyncSortingOrder),
                typeof(SyncColor),
                typeof(SyncLocalPosition),
                typeof(SyncAnimator),
                typeof(SyncScrollRect),
                typeof(SyncGroup),
#if !USE_OLD_TEXT
                typeof(SyncTextMeshPro)
#endif
            };

            foreach(var type in regTypes)
            {
                var action = (ISyncAction)System.Activator.CreateInstance(type);
                Register(type.Name, action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ISyncAction GetAction(string typeName)
        {
            if (!factorys.ContainsKey(typeName))
                throw new System.NullReferenceException($"no implement sync action by type {typeName}");

            return factorys[typeName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncAction"></param>
        public void Register(string typeName, ISyncAction syncAction)
        {
            if (!factorys.ContainsKey(typeName))
                factorys.Add(typeName, syncAction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncAction"></param>
        public void Unregister(string typeName)
        {
            if (factorys.ContainsKey(typeName))
                factorys.Remove(typeName);
        }
    }
}
