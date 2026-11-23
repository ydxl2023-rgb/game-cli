using Oathx.Widget;

/// <summary>Present the character page and publish its user actions.</summary>
[Config("Assets/Data/Widgets/Character/UICharacter.prefab", WidgetLayer.center, 0, true, true, true)]
public class UICharacter : UIWidget<VMCharacter>
{
    /// <summary>Bind the close action to the current view model.</summary>
    protected override bool Bind()
    {
        vm.Close = Send<ReqCharacterClose>;

        return true;
    }

    /// <summary>Release the close action before the page is rebound.</summary>
    public override void Unref(VM refVM)
    {
        if (vm)
        {
            vm.Close = null;
            base.Unref(refVM);
        }
    }
}

/// <summary>The player requested that the character page close.</summary>
public class ReqCharacterClose : UIRequestEvent
{
}
