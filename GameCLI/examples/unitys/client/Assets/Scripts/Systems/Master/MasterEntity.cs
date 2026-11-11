using Config;
using Newtonsoft.Json;
using Oathx.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// master property define
/// </summary>
public class MasterPropertyDef
{
    public const int PID = 0;
    public const int LV = 1;
    public const int VIP = 2;
    public const int MONEY = 3;
    public const int DIAMOND = 4;
    public const int CHAPTER = 5;
    public const int LEVEL = 6;
}

/// <summary>
/// master entity
/// </summary>
[Initialize]
public class MasterEntity : XEntitySerialize
{
    /// <summary>
    /// constructor
    /// </summary>
    public MasterEntity()
    {
        InitAttrs();
    }

    /// <summary>
    /// init master attrs
    /// </summary>
    private void InitAttrs()
    {
        var sampleAttrs = new Dictionary<int, int>() {
            { MasterPropertyDef.PID, 0 },
            { MasterPropertyDef.LV, 1 },
            { MasterPropertyDef.VIP, 1 },
            { MasterPropertyDef.MONEY, 3000 },
            { MasterPropertyDef.DIAMOND, 100 },
            { MasterPropertyDef.CHAPTER, Excel.CHAPTER_HOME},
            { MasterPropertyDef.LEVEL, 0 }
        };

        ApplyAttrs(sampleAttrs);
    }

    /// <summary>
    /// try get chapter and level
    /// </summary>
    /// <param name="levelId"></param>
    /// <returns></returns>
    public int TryGetChapter(out int levelId)
    {
        levelId = GetAttr(MasterPropertyDef.LEVEL);
        return GetAttr(MasterPropertyDef.CHAPTER);
    }
}
