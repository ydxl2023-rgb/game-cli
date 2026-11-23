using Oathx.Widget;
using Config;

[Config("Assets/Data/Widgets/SevenDayLogin/UISevenDayLogin.prefab", WidgetLayer.dialog)]
public class UISevenDayLogin : UIWidget<VMSevenDayLogin>
{

    public enum Clip
    {
        idle,
        open,
        close
    }

    /// <summary>Bind local sign-in controls to typed player intents.</summary>
    protected override bool Bind()
    {
        vm.FancyClick = (sender, index) => vm.Days != null && index >= 0 && index < vm.Days.Count && Select(vm.Days[index].day);
        vm.Claim = Send<ReqSevenDayLoginClaim>;
        vm.Close = Send<ReqSevenDayLoginClose>;

        return true;
    }

    /// <summary>
    /// check if the seven day login entity is valid and can show the widget
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool Invalidate(SevenDayLoginEntity entity)
    {
        if (!entity || !vm)
            return false;

        VList<VMSevenDayLoginSlot> slots = new VList<VMSevenDayLoginSlot>();
        foreach(var day in entity.days)
        {
            var prop = Excel.propSevenDayLoginById(day.day);
            slots.Add(new VMSevenDayLoginSlot() { 
                day = day.day,
                Title = prop.Name
            });
        }
        vm.Days = slots;

        return true;
    }

    /// <summary>
    /// handle click event for claiming reward, 
    /// check if the day is valid and update the entity accordingly
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public bool Select(int day)
    {
        return Send(new ReqSevenDayLoginSelectDay { day = day });
    }

    /// <summary>
    /// handle claim reward success, 
    /// update the entity and play the corresponding animation or effect based on the claimed day
    /// </summary>
    /// <param name="day"></param>
    public void ClaimSucceed(int day)
    {
    }
}

/// <summary>Request selection of a visible sign-in day.</summary>
public class ReqSevenDayLoginSelectDay : UIRequestEvent
{
    /// <summary>Stable configured day number.</summary>
    public int day
    { get; set; }
}

/// <summary>Request the reward for the selected sign-in day.</summary>
public class ReqSevenDayLoginClaim : UIRequestEvent
{
}

/// <summary>Request closing the sign-in page.</summary>
public class ReqSevenDayLoginClose : UIRequestEvent
{
}
