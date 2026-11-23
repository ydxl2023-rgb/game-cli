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
    /// <summary>Stable serialized pid attribute key.</summary>
    public const int pid = 0;

    /// <summary>Stable serialized lv attribute key.</summary>
    public const int lv = 1;

    /// <summary>Stable serialized vip attribute key.</summary>
    public const int vip = 2;

    /// <summary>Stable serialized money attribute key.</summary>
    public const int money = 3;

    /// <summary>Stable serialized diamond attribute key.</summary>
    public const int diamond = 4;

    /// <summary>Stable serialized chapter attribute key.</summary>
    public const int chapter = 5;

    /// <summary>Stable serialized level attribute key.</summary>
    public const int level = 6;
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
    /// try get chapter and level
    /// </summary>
    /// <param name="levelId">Stored level within the chapter.</param>
    /// <returns>The stored chapter identifier.</returns>
    public int TryGetChapter(out int levelId)
    {
        levelId = GetAttr(MasterPropertyDef.level);

        return GetAttr(MasterPropertyDef.chapter);
    }

    /// <summary>Apply a validated local login identity without changing other attributes.</summary>
    /// <param name="playerId">Positive player identity from the login event.</param>
    /// <returns>True when the identity is valid and was applied.</returns>
    public bool AcceptIdentity(int playerId)
    {
        if (playerId <= 0)
            return false;

        ApplyAttrs(new Dictionary<int, int> { { MasterPropertyDef.pid, playerId } });

        return true;
    }

    /// <summary>
    /// init master attrs
    /// </summary>
    private void InitAttrs()
    {
        var sampleAttrs = new Dictionary<int, int>() {
            { MasterPropertyDef.pid, 1 },
            { MasterPropertyDef.lv, 1 },
            { MasterPropertyDef.vip, 1 },
            { MasterPropertyDef.money, 3000 },
            { MasterPropertyDef.diamond, 100 },
            { MasterPropertyDef.chapter, Excel.CHAPTER_HOME},
            { MasterPropertyDef.level, 0 }
        };

        ApplyAttrs(sampleAttrs);
    }

}

