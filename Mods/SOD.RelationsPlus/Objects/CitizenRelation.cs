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

                    if (Plugin.Instance.Config.DebugMode)
                        Plugin.Log.LogInfo($"[Debug]: Citizen({CitizenId}|{CityData.Instance.citizenDictionary[CitizenId].GetCitizenName()}): Changed 'Know' value from \"{oldValue}\" to \"{newValue}\".");

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

                    if (Plugin.Instance.Config.DebugMode)
                        Plugin.Log.LogInfo($"[Debug]: Citizen({CitizenId}|{CityData.Instance.citizenDictionary[CitizenId].GetCitizenName()}): Changed 'Like' value from \"{oldValue}\" to \"{newValue}\".");

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
                Plugin.Log.LogInfo($"[Debug]: Citizen({CitizenId}|{CityData.Instance.citizenDictionary[CitizenId].GetCitizenName()}): saw player at \"{location}\" on \"{LastSeenGameTime}\".");

            // Raise events
            SeenPlayerArgs args = null; 
            OnPlayerSeen?.Invoke(this, args ??= new SeenPlayerArgs(CitizenId, location, knowChange, likeChange));
            RelationManager.Instance.RaiseEvent(RelationManager.EventName.Seen, args ?? new SeenPlayerArgs(CitizenId, location, knowChange, likeChange));
        }
    }
}