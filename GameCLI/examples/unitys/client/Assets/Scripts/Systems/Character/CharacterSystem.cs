using Oathx.Widget;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// character system
/// </summary>
public class CharacterSystem : RuntimeSystem
{
    /// <summary>
    /// active character system
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        UISystem.GetSingleton().OpenWidget<UICharacter>(widget => {
            if (widget)
            {
                // default hide character ui
                widget.Deactive();
                
                SetupClicks(widget);
            }
        });
        return true;
    }

    /// <summary>
    /// close character system
    /// </summary>
    public override void OnDetive()
    {
        System.Type[] widgets = new System.Type[] {
            typeof(UICharacter)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }

    /// <summary>
    /// setup character all click callbacks
    /// </summary>
    /// <param name="widget"></param>
    /// <returns></returns>
    private bool SetupClicks(UICharacter widget)
    {
        var vm = widget.GetVM() as VMCharacter;
        if (!vm)
            return false;

        SetupCloseClicks(vm);
        
        return true;
    }

    /// <summary>
    /// close button clicked
    /// </summary>
    /// <param name="vm"></param>
    private void SetupCloseClicks(VMCharacter vm) 
    {
        vm.Close = (sender) => {
            System.Type[] widgets = new System.Type[] {
                typeof(UICharacter)
            };

            UISystem.GetSingleton().CloseWidget(widgets);

            return true;
        };
    }
}
