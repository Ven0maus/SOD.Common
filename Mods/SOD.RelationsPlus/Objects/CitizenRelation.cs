using SOD.Common;
using SOD.Common.Helpers;
using System;
using System.Text.Json.Serialization;

namespace SOD.RelationsPlus.Objects
{
    /// <summary>
    /// Contains information about the relation between the player and the citizen.
    /// </summary>
    public sealed class CitizenRelation
    {
        /// <summary>
        /// Raised when Know property is changed for this specific citizen.
        /// </summary>
        public event EventHandler<RelationChangeArgs> OnKnowChanged;

        /// <summary>
        /// Raised when Like property is changed for this specific citizen.
        /// </summary>
        public event EventHandler<RelationChangeArgs> OnLikeChanged;

        /// <summary>
        /// Raised when this specific citizen sees the player.
        /// </summary>
        public event EventHandler<SeenPlayerArgs> OnPlayerSeen;

        /// <summary>
        /// The citizen's ID
        /// </summary>
        public int CitizenId { get; }

        /// <summary>
        /// The last time the citizen has seen the player in real time.
        /// </summary>
        public DateTime? LastSeenRealTime { get; private set; }

        /// <summary>
        /// The last time the citizen has seen the player in game time.
        /// </summary>
        public Time.TimeData? LastSeenGameTime { get; private set; }

        /// <summary>
        /// Determines if the player was trespassing the last time he was seen by the citizen.
        /// </summary>
        public bool WasTrespassingLastTimeSeen { get; internal set; } = false;

        /// <summary>
        /// Get's the current citizens know relation with the player based on the know gates configured.
        /// </summary>
        [JsonIgnore]
        public KnowStage KnowRelation 
        {
            get 
            {
                if (Know < Plugin.Instance.Config.KnowGateOne)
                    return KnowStage.Stranger;
                if (Know < Plugin.Instance.Config.KnowGateTwo)
                    return KnowStage.Aware;
                if (Know < Plugin.Instance.Config.KnowGateThree)
                    return KnowStage.Familiar;
                if (Know < Plugin.Instance.Config.KnowGateFour)
                    return KnowStage.Acquaintance;
                return KnowStage.Friend;
            }
        }

        /// <summary>
        /// Get's the current citizens like relation with the player based on the like gates configured.
        /// </summary>
        [JsonIgnore]
        public LikeStage LikeRelation
        {
            get
            {
                if (Like < Plugin.Instance.Config.LikeGateOne)
                    return LikeStage.Hated;
                if (Like < Plugin.Instance.Config.LikeGateTwo)
                    return LikeStage.Disliked;
                if (Like < Plugin.Instance.Config.LikeGateThree)
                    return LikeStage.Neutral;
                if (Like < Plugin.Instance.Config.LikeGateFour)
                    return LikeStage.Liked;
                return LikeStage.Loved;
            }
        }

        private float _know = 0f;
        /// <summary>
        /// How much the citizen knows the player. (how often seen, interacted)
        /// <br>Range between 0-1 (0: unknown, 1: well known)</br>
        /// <br>Default value: 0</br>
        /// </summary>
        public float Know
        {
            get => _know;
            set
            {
                var oldValue = _know;
                var newValue = UnityEngine.Mathf.Clamp01(value);
                if (_know != newValue)
                {
                    _know = newValue;

                    // Skip when loading data from savefile
                    if (RelationManager.Instance.IsLoading) return;
                    if (Plugin.Instance.Config.DebugMode)
                        Plugin.Log.LogInfo($"[Debug]: Citizen({CitizenId}|{GetCitizen()?.GetCitizenName() ?? "Unknown"}): Changed 'Know' value from \"{oldValue}\" to \"{newValue}\" | KnowRelation: \"{KnowRelation}\".");

                    RelationChangeArgs args = null;
                    OnKnowChanged?.Invoke(this, args ??= new RelationChangeArgs(CitizenId, oldValue, newValue));
                    RelationManager.Instance.RaiseEvent(RelationManager.EventName.KnowChange, args ?? new RelationChangeArgs(CitizenId, oldValue, newValue));
                }
            }
        }

        private float _like = 0.5f;
        /// <summary>
        /// How much the citizen actually likes the player. (From actively doing something for the citizen)
        /// <br>Range between 0-1 (0: dislike, 0.5 neutral, 1: like)</br>
        /// <br>Default value: 0.5</br>
        /// </summary>
        public float Like
        {
            get => _like;
            set
            {
                var oldValue = _like;
                var newValue = UnityEngine.Mathf.Clamp01(value);
                if (_like != newValue)
                {
                    _like = newValue;

                    // Skip when loading data from savefile
                    if (RelationManager.Instance.IsLoading) return;
                    if (Plugin.Instance.Config.DebugMode)
                        Plugin.Log.LogInfo($"[Debug]: Citizen({CitizenId}|{GetCitizen()?.GetCitizenName() ?? "Unknown"}): Changed 'Like' value from \"{oldValue}\" to \"{newValue}\" | LikeRelation: \"{LikeRelation}\".");

                    RelationChangeArgs args = null;
                    OnLikeChanged?.Invoke(this, args ??= new RelationChangeArgs(CitizenId, oldValue, newValue));
                    RelationManager.Instance.RaiseEvent(RelationManager.EventName.LikeChange, args ?? new RelationChangeArgs(CitizenId, oldValue, newValue));
                }
            }
        }

        [JsonConstructor]
        public CitizenRelation(int citizenId)
        {
            CitizenId = citizenId;
        }

        /// <summary>
        /// Retrieves the cached citizen object from the city data instance.
        /// Could be null if the citizen is missing from the citydata somehow (handled by the game).
        /// </summary>
        /// <returns></returns>
        public Citizen GetCitizen()
        {
            if (!CityData.Instance.citizenDictionary.TryGetValue(CitizenId, out var human))
                return null;
            return human as Citizen;
        }

        /// <summary>
        /// Raises required seen event.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="knowChange"></param>
        /// <param name="likeChange"></param>
        internal void RaiseSeenEvent(SeenPlayerArgs.SeenLocation location, float knowChange, float likeChange)
        {
            LastSeenRealTime = DateTime.Now;
            LastSeenGameTime = Lib.Time.CurrentDateTime;

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Debug]: Citizen({CitizenId}|{GetCitizen()?.GetCitizenName() ?? "Unknown"}): saw player at \"{location}\" on \"{LastSeenGameTime}\".");

            // Raise events
            SeenPlayerArgs args = null; 
            OnPlayerSeen?.Invoke(this, args ??= new SeenPlayerArgs(CitizenId, location, knowChange, likeChange));
            RelationManager.Instance.RaiseEvent(RelationManager.EventName.Seen, args ?? new SeenPlayerArgs(CitizenId, location, knowChange, likeChange));
        }

        /// <summary>
        /// The different know stages between citizens and player.
        /// </summary>
        public enum KnowStage
        {
            Stranger,
            Aware,
            Familiar,
            Acquaintance,
            Friend,
        }

        /// <summary>
        /// The different like stages between citizens and player.
        /// </summary>
        public enum LikeStage
        {
            Neutral,
            Hated,
            Disliked,
            Liked,
            Loved
        }
    }
}