using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Widget
{
    [Binding(typeof(FancyAnimatorView))]
    public class SyncFancyAnimatorView : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyAnimatorView fancyAnimatorView)
            {
                if (value is IContainer container)
                {
                    container.containerChange += (sender, opCode, index, element) => {
                        switch (opCode)
                        {
                            case ContainerOpCode.Added:
                                fancyAnimatorView.Add(element);
                                break;

                            case ContainerOpCode.Remove:
                                fancyAnimatorView.Remove(element);
                                break;
                        }
                    };

                    if (container.TryToArray(out var elements))
                    {
                        fancyAnimatorView.UpdateData(elements);
                    }
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [FancyAnimatorScrollView] do not match");
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

    public delegate bool FancyAnimatorClick(FancyAnimatorView sender, int index);

    /// <summary>
    /// 
    /// </summary>
    [Binding(typeof(FancyAnimatorView))]
    public class SyncFancyAnimatorClick : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyAnimatorView c)
            {
                if (value is FancyAnimatorClick click)
                {
                    c.SetCellClicked((v) => {
                        click?.Invoke(c, v);
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {c.name} failure" +
                           $" Invalid cast {value.GetType()} to {typeof(FancyClick).Name}");
                }
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