using System.Collections.Generic;
using Oathx.Entity;

/// <summary>Owns the town's logical actor collection and advances it through the framework clock.</summary>
[Initialize(InstallType.Custom, true)]
public sealed class TownEntity : XEntity
{
    /// <summary>Actors keyed by stable instance identity, with no scene object references.</summary>
    private readonly Dictionary<int, ActorEntity> actors = new Dictionary<int, ActorEntity>();

    /// <summary>Number of actors currently owned by this town.</summary>
    public int actorCount
    { get { return actors.Count; } }

    /// <summary>Advances actors whether or not their shapes are visible or enabled.</summary>
    /// <param name="elapsed">Elapsed simulation time from the framework clock.</param>
    public override void Update(float elapsed)
    {
        foreach (ActorEntity actor in actors.Values)
            actor.Update(elapsed);
    }

    /// <summary>Destroys owned actors before releasing framework components.</summary>
    public override void Destroy()
    {
        Clear();

        base.Destroy();
    }

    /// <summary>Atomically installs validated actor data into an empty town.</summary>
    /// <param name="spawns">Detached actor definitions supplied by scene authoring.</param>
    /// <returns>Whether every definition was valid and installed.</returns>
    public bool Initialize(ActorSpawn[] spawns)
    {
        if (actors.Count != 0 || spawns == null || spawns.Length == 0)
            return false;

        var candidates = new Dictionary<int, ActorEntity>();
        foreach (ActorSpawn spawn in spawns)
        {
            var actor = new ActorEntity();
            if (!actor.Initialize(spawn) || candidates.ContainsKey(actor.actorId))
            {
                actor.Destroy();
                foreach (ActorEntity candidate in candidates.Values)
                    candidate.Destroy();

                return false;
            }

            candidates.Add(actor.actorId, actor);
        }

        foreach (var candidate in candidates)
            actors.Add(candidate.Key, candidate.Value);

        return true;
    }

    /// <summary>Returns one logical actor without exposing the mutable collection.</summary>
    /// <param name="actorId">Stable actor instance identifier.</param>
    /// <returns>The actor, or null when absent.</returns>
    public ActorEntity FindActor(int actorId)
    {
        return actors.TryGetValue(actorId, out var actor) ? actor : null;
    }

    /// <summary>Releases all owned actor data and invalidates detached references.</summary>
    public void Clear()
    {
        foreach (ActorEntity actor in actors.Values)
            actor.Destroy();

        actors.Clear();
    }
}
