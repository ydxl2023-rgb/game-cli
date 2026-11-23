using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Widget
{
    [Binding(typeof(FancyView))]
    public class SyncFancyView : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyView c)
            {
                if (value is IContainer container)
                {
                    container.containerChange += (sender, opCode, index, element) =>
                    {
                        if (component is FancyView scrollView)
                        {
                            switch (opCode)
                            {
                                case ContainerOpCode.Added:
                                    scrollView.Add(element);
                                    break;

                                case ContainerOpCode.Remove:
                                    scrollView.Remove(element);
                                    break;
                            }
                        }
                    };

                    if (container.TryToArray(out var elements))
                    {
                        c.UpdateData(elements);
                    }
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [FancyScrollView] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is IWidget widget)
                {
                    var vm = widget.GetVM();
                    if (vm)
                        widget.Unref(vm);
                }
            }
        }
    }

    [Binding(typeof(FancyView))]
    public class SyncFancyJumpTo : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyView c)
            {
                if (value is Boxed<int> v)
                {
                    c.JumpTo(v.value);
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {c.name} failure" +
                        $" Invalid cast {value.GetType()} to {typeof(Boxed<int>).Name}");
                }
            }
        }

        public void UnSync(Property property)
        {

        }
    }

    [Binding(typeof(FancyView))]
    public class SyncFancySelect : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyView c)
            {
                if (value is Boxed<int> v)
                {
                    c.SetSelectIndex(v.value);
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {c.name} failure" +
                            $" Invalid cast {value.GetType()} to {typeof(Boxed<int>).Name}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [FancyView] do not match");
            }
        }

        public void UnSync(Property property)
        {

        }
    }


    [Binding(typeof(FancyView))]
    public class SyncFancyScrollTo : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyView c)
            {
                if (value is Boxed<int> v)
                {
                    c.ScrollTo(v.value, 0.1f, EasingCore.Ease.Linear);
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {c.name} failure" +
                           $" Invalid cast {value.GetType()} to {typeof(Boxed<int>).Name}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [FancyView] do not match");
            }
        }

        public void UnSync(Property property)
        {

        }
    }

    public delegate bool FancyClick(FancyView sender, int index);

    /// <summary>
    /// 
    /// </summary>
    [Binding(typeof(FancyView))]
    public class SyncFancyClick : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyView c)
            {
                if (value is FancyClick click)
                {
                    c.SetCellClicked((v)=> {
                        click?.Invoke(c, v);
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {c.name} failure" +
                           $" Invalid cast {value.GetType()} to {typeof(FancyClick).Name}");
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [FancyView] do not match");
            }
        }

        public void UnSync(Property property)
        {
            foreach (var component in property.components)
            {
                if (component is FancyView fancyView)
                {
                    fancyView.SetCellClicked(null);
                }
            }
        }
    }
}