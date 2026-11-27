using System.Collections.Generic;
using System.Linq;
using Oathx.Entity;

/// <summary>Owns persistent hero instances independently of their scene actors.</summary>
public sealed class HeroCollectionEntity : XEntity
{
    /// <summary>Owned additional heroes keyed by configuration identity.</summary>
    private readonly Dictionary<int, HeroEntity> members = new Dictionary<int, HeroEntity>();

    /// <summary>The existing primary hero, including its pre-recruitment empty identity.</summary>
    public HeroEntity primary
    { get; private set; }

    /// <summary>Number of actually recruited heroes.</summary>
    public int count
    { get { return members.Count + (primary.configurationId == 0 ? 0 : 1); } }

    /// <summary>Creates the roster from the existing aggregate save.</summary>
    /// <param name="saved">Committed village save.</param>
    public HeroCollectionEntity(HomeVillageSave saved)
    {
        primary = new HeroEntity(saved);
        Restore(saved);
    }

    /// <summary>Releases each owned hero exactly once.</summary>
    public override void Destroy()
    {
        primary.Destroy();
        foreach (var hero in members.Values)
            hero.Destroy();

        members.Clear();

        base.Destroy();
    }

    /// <summary>Checks actual hero ownership rather than task completion or visible models.</summary>
    /// <param name="heroId">Hero configuration identity.</param>
    /// <returns>Whether this hero is in the persistent roster.</returns>
    public bool Contains(int heroId)
    {
        return heroId != 0 && (primary.configurationId == heroId || members.ContainsKey(heroId));
    }

    /// <summary>Returns owned hero identities without exposing the mutable collection.</summary>
    /// <returns>A detached identity list.</returns>
    public int[] GetHeroIds()
    {
        var ids = members.Keys.ToList();
        if (primary.configurationId != 0)
            ids.Insert(0, primary.configurationId);

        return ids.ToArray();
    }

    /// <summary>Updates primary data and materializes newly committed recruits.</summary>
    /// <param name="saved">Authoritative village snapshot.</param>
    public void Restore(HomeVillageSave saved)
    {
        primary.Restore(saved);
        foreach (int id in saved.recruit.heroes)
        {
            if (members.ContainsKey(id))
                continue;

            var definition = Config.Excel.recruitHeroConfig[id];

            var data = new HomeVillageSave {
                heroId = id,
                healthPercent = definition.InitialHealthPercent,
                heroGold = definition.InitialGold
            };

            members.Add(id, new HeroEntity(data));
        }
    }
}
