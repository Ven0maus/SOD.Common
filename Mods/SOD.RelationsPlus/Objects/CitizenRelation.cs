using SOD.Common;
using SOD.Common.Helpers;
using System;
using System.Collections.Generic;

namespace SOD.RelationsPlus.Objects
{
    /// <summary>
    /// Contains information about the relation between the player and the citizen.
    /// </summary>
    public class CitizenRelation
    {
        /// <summary>
        /// Raised when Know property is changed.
        /// </summary>
        public event EventHandler<RelationChangeArgs> OnKnowChanged;

        /// <summary>
        /// Raised when Like property is changed.
        /// </summary>
        public event EventHandler<RelationChangeArgs> OnLikeChanged;

        /// <summary>
        /// Raised when the citizen sees the player.
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
                    OnKnowChanged?.Invoke(this, new RelationChangeArgs(oldValue, newValue));
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
                    OnLikeChanged?.Invoke(this, new RelationChangeArgs(oldValue, newValue));
                }
            }
        }

        /// <summary>
        /// Hosts custom modifiers that can be handled by mods.
        /// </summary>
        private Dictionary<string, float> _customModifiers;

        internal CitizenRelation(int citizenId)
        {
            CitizenId = citizenId;
        }

        /// <summary>
        /// Get's the given custom modifier by key, exception if not found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public float GetModifier(string key)
        {
            if (_customModifiers.TryGetValue(key, out var modifier))
                return modifier;
            throw new KeyNotFoundException($"No custom modifer found with key \"{key}\".");
        }

        /// <summary>
        /// Determine's if a custom modifier exists with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ModifierExists(string key)
            => _customModifiers?.ContainsKey(key) ?? false;

        /// <summary>
        /// Add's or updates a custom modifier by key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdateModifier(string key, float value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            _customModifiers ??= new Dictionary<string, float>();
            _customModifiers[key] = value;
        }

        /// <summary>
        /// Removes a custom modifier by key.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveModifier(string key)
            => _customModifiers?.Remove(key);

        /// <summary>
        /// Retrieves all the existing custom modifiers.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, float> GetModifiers()
            => _customModifiers ?? new Dictionary<string, float>();

        /// <summary>
        /// Raises required seen event.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="knowChange"></param>
        /// <param name="likeChange"></param>
        internal void Seen(SeenPlayerArgs.SeenLocation location, float knowChange, float likeChange)
        {
            LastSeenRealTime = DateTime.Now;
            LastSeenGameTime = Lib.Time.CurrentDateTime;
            OnPlayerSeen?.Invoke(this, new SeenPlayerArgs(location, knowChange, likeChange));
        }
    }
}