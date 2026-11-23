using Oathx.Core;
using Oathx.Widget;

/// <summary>Coordinate the character page lifecycle.</summary>
public class CharacterSystem : RuntimeSystem
{
    /// <summary>Prepare the character page and keep it hidden until requested.</summary>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UnsubscribeAll();

        Subscribe<ReqCharacterClose>(request => ClosePage());

        UISystem.GetSingleton().OpenWidget<UICharacter>(widget => {
            if (widget)
                widget.Deactive();
        });

        return true;
    }

    /// <summary>Release subscriptions and close the character page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        ClosePage();
    }

    /// <summary>Close the page through the system-level widget API.</summary>
    private void ClosePage()
    {
        System.Type[] widgets = new System.Type[] {
            typeof(UICharacter)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }
}
