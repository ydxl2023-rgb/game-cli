using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using System.Collections.Generic;

/// <summary>
/// backpack event, used to control the display of the backpack UI, with a boolean property 'show' that indicates whether to show or hide the backpack UI.
/// </summary>
public class BackpackEvent : IEvent
{
    /// <summary>
    /// flag to indicate whether to show the backpack UI
    /// </summary>
    public bool show
    { get; set; }
}

/// <summary>
/// grab item event, used to trigger the action of grabbing items into the backpack,
/// with a list of ItemEntity objects representing the items to be grabbed.
/// </summary>
public class GrabItemEvent : IEvent
{
    /// <summary>
    /// list of items to be grabbed into the backpack
    /// </summary>
    public List<ItemEntity> items 
    { get; set; } = new List<ItemEntity>();
}

/// <summary>
/// backpack system, responsible for managing the player's backpack, including handling backpack events to show or hide the backpack UI,
/// </summary>
public class BackpackSystem : RuntimeSystem
{
    /// <summary>
    /// player backpack entity
    /// </summary>
    public BackpackEntity entity
    {  get; private set; }

    /// <summary>
    /// install backpack system, loads the backpack entity for the player using their player id (PID) obtained from the master entity,
    /// </summary>
    /// <returns></returns>
    public override bool Install()
    {
        // subscribe events
        Subscribe<BackpackEvent>(OnBackpackEvent);
        Subscribe<GrabItemEvent>(OnGrabItemEvent);

        return true;
    }

    /// <summary>
    /// uninstall backpack system, currently does nothing, but can be used to clean up resources or unsubscribe from events when the backpack system is uninstalled
    /// </summary>
    public override void Uninstall()
    {
        UnsubscribeAll();
    }

    /// <summary>
    /// backpack system active
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (!master)
            return false;

        var pid = master.GetAttr(MasterPropertyDef.PID);
        if (pid != 0)
        {
            Rpc.Load<BackpackEntity>(pid, (code, backpack) => {
                entity = backpack;
            });
        }

        return true;
    }

    /// <summary>
    /// backpack system detive
    /// </summary>
    public override void OnDetive()
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (master)
        {
             Rpc.Save(master.GetAttr(MasterPropertyDef.PID), entity);
        }

        System.Type[] widgets = new System.Type[] {
            typeof(UICharacter)
        };

        UISystem.GetSingleton().CloseWidget(widgets);
    }

    /// <summary>
    /// backpack event handler, open or close the backpack UI based on the 'show' property of the event
    /// </summary>
    /// <param name="evt"></param>
    private void OnBackpackEvent(BackpackEvent evt)
    {
        if (evt.show)
        {
            UISystem.GetSingleton().OpenWidget<UICharacter>(widget => { 
                
            });
        }
        else
        {
            System.Type[] widgets = new System.Type[] {
                typeof(UICharacter)
            };

            UISystem.GetSingleton().CloseWidget(widgets);
        }
    }

    /// <summary>
    /// grab item event handler, 
    /// trigger the backpack entity to grab the items specified in the event's 'items' property
    /// </summary>
    /// <param name="evt"></param>
    private void OnGrabItemEvent(GrabItemEvent evt)
    {
        if (entity)
        {
            entity.GrabItems(evt.items);
        }
    }
}
