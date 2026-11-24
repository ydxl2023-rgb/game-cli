using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Oathx.Widget
{

    [Binding(typeof(FancyGridScrollView))]
    public class SyncFancyGridScrollView : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyGridScrollView fancyGridView)
            {
                if (value is IContainer container)
                {
                    container.containerChange += (sender, opCode, index, element) => {
                        switch (opCode)
                        {
                            case ContainerOpCode.Added:
                                fancyGridView.Add(element);
                                break;

                            case ContainerOpCode.Remove:
                                fancyGridView.Remove(element);
                                break;
                        }
                    };

                    if (container.TryToArray(out var elements))
                    {
                        fancyGridView.UpdateContents(elements);
                    }
                }
            }
            else
            {
                throw new System.ArgumentNullException($"The bound component {component.GetType().Name} and synchronizer [FancyGridView] do not match");
            }
        }

        public void UnSync(Property property)
        {            
        }
    }

    public delegate bool FancyGridClick(FancyGridScrollView sender, int index);

    [Binding(typeof(FancyGridScrollView))]
    public class SyncFancyGridClick : ISyncAction
    {
        public void Sync(string propertyName, object value, Property property, Component component)
        {
            if (component is FancyGridScrollView c)
            {
                if (value is FancyGridClick click)
                {
                    c.SetCellClicked((v) => {
                        click?.Invoke(c, v);
                    });
                }
                else
                {
                    throw new System.InvalidCastException($"Sync property {propertyName} to {c.name} failure" +
                           $" Invalid cast {value.GetType()} to {typeof(FancyGridClick).Name}");
                }
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
}