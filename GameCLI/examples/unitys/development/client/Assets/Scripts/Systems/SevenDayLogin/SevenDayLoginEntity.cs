using Config;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using System;
using System.Collections.Generic;

public static class SevenDayLoginDef
{
    public const int DAY_TOTAY = 31;
}

/// <summary>
/// day status, used to represent the status of a login day, such as whether the player has signed in on that day or not.
/// </summary>
public enum DayStatus
{
    /// <summary>
    /// unsigned the player has not signed in on that day and can claim the reward if eligible.
    /// </summary>
    None,
    /// <summary>
    // signed, the player has signed in on that day and can claim the reward if eligible.
    /// </summary>
    Sign,
}

[Serializable]
public class DayInfo
{
    /// <summary>
    /// day number, used to represent the specific day in the seven day login event, 
    /// such as day 1, day 2, etc.
    /// </summary>
    public int day
    { get; set; }

    /// <summary>
    /// day status, used to represent the status of a login day,
    /// such as whether the player has signed in on that day or not.
    /// </summary>
    public DayStatus status
    { get; set; } = DayStatus.None;
}

/// <summary>
/// seven day login entity, 
/// responsible for managing the player's seven day login data,
/// including tracking login days and times,
/// </summary>
[Initialize]
public class SevenDayLoginEntity : XEntitySerialize
{
    /// <summary>
    /// login days, used to track the player's login status for each day in the seven day login event,
    /// </summary>
    [JsonProperty]
    public List<DayInfo> days
    { get; protected set; } = new List<DayInfo>();

    /// <summary>
    /// Transient sign-in day selected on the open page.
    /// </summary>
    [JsonIgnore]
    public int? selectedDay
    { get; set; }

    /// <summary>
    /// login days, used to track the number of days the player has logged in,
    /// </summary>
    public SevenDayLoginEntity()
    {
        InitProps();
    }

    /// <summary>
    /// initialize properties, used to set up the initial state of the seven day login entity,
    /// </summary>
    private void InitProps()
    {
        foreach (var item in Excel.sevenDayLoginConfig)
        {
            if (days.FindIndex(d => d.day == item.Key) != -1)
                continue;

            days.Add(new DayInfo() {
                day = item.Key, status = DayStatus.None
            });
        }
    }

    /// <summary>
    /// try to get day info by day number, used to retrieve the login status for a specific day in the seven day login event,
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public DayInfo GetDay(int day)
    {
        if (day >= 0 && day < days.Count)
        {
            return days[day];
        }

        return null;
    }
}
