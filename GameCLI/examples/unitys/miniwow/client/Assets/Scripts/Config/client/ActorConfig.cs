using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Actor
    {
        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public string Model { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float MoveSpeed { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float JoystickRadius { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float HorizontalScale { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float FollowRate { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float SpawnX { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float SpawnY { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float SpawnZ { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float ModelScale { get; private set; }

        /// <summary>
        /// Town player configuration
        /// </summary>
        [JsonProperty]
        public float AgentRadius { get; private set; }

        /// <summary>
        /// AgentTypeId
        /// </summary>
        [JsonProperty]
        public int AgentTypeId { get; private set; }

        /// <summary>
        /// AgentHeight
        /// </summary>
        [JsonProperty]
        public float AgentHeight { get; private set; }

        /// <summary>
        /// JoystickSourceWidth
        /// </summary>
        [JsonProperty]
        public float JoystickSourceWidth { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Actor exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ActorConfig : Config<Actor>
    {
    }

    /// <summary>
    /// export excel Actor data
    /// </summary>
    public sealed partial class Excel
    {
        public const int ACTOR_TOWN_PLAYER = 10001;
        /// <summary>
        /// config ActorConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Actor> actorConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Actor propActorById(int nId)
        {
            Actor data;
            if (!actorConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Actor).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}