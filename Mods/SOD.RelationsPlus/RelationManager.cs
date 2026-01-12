using SOD.Common;
using SOD.Common.Helpers;
using SOD.RelationsPlus.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SOD.RelationsPlus
{
    /// <summary>
    /// Contains methods to create, update and delete relations between citizens and load/save the relation state per savegame.
    /// </summary>
    public sealed class RelationManager : IEnumerable<CitizenRelation>
    {
        private static RelationManager _instance;
        /// <summary>
        /// The instance accessor for the RelationManager class
        /// </summary>
        public static RelationManager Instance => _instance ??= new RelationManager();

        /// <summary>
        /// A relation matrix dictionary for each unique citizen.
        /// </summary>
        private readonly Dictionary<int, CitizenRelation> _relationMatrixes = new();

        /// <summary>
        /// Stores the last check time of the timed decay logic.
        /// </summary>
        private int _lastDecayCheckMinute;

        /// <summary>
        /// Custom indexer to get citizen relation information, if none exists it will create a new relation object.
        /// <br>This indexer will always return either an existing value or a new value.</br>
        /// </summary>
        /// <param name="citizenId"></param>
        /// <returns></returns>
        public CitizenRelation this[int citizenId]
        {
            get { return GetOrCreate(citizenId); }
        }

        /// <summary>
        /// Raised when Know property is changed for a citizen.
        /// </summary>
        public event EventHandler<RelationChangeArgs> OnKnowChanged;

        /// <summary>
        /// Raised when Like property is changed for a citizen.
        /// </summary>
        public event EventHandler<RelationChangeArgs> OnLikeChanged;

        /// <summary>
        /// Raised when a citizen sees the player.
        /// </summary>
        public event EventHandler<SeenPlayerArgs> OnPlayerSeen;

        private RelationManager()
        { }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="citizenId"></param>
        /// <param name="relation"></param>
        /// <returns></returns>
        public bool TryGetValue(int citizenId, out CitizenRelation relation)
            => _relationMatrixes.TryGetValue(citizenId, out relation);

        /// <summary>
        /// Determines if the specified key exists.
        /// </summary>
        /// <param name="citizenId"></param>
        /// <returns></returns>
        public bool ContainsKey(int citizenId) 
            => _relationMatrixes.ContainsKey(citizenId);

        /// <summary>
        /// Remove's relation information of a citizen.
        /// </summary>
        /// <param name="citizenId"></param>
        public void Remove(int citizenId)
        {
            _relationMatrixes.Remove(citizenId);
        }

        /// <summary>
        /// Retrieves the relational information of a citizen, creates a new object if it doesn't exist yet.
        /// </summary>
        /// <param name="citizenId"></param>
        /// <returns></returns>
        public CitizenRelation GetOrCreate(int citizenId)
        {
            if (!_relationMatrixes.TryGetValue(citizenId, out var relationMatrix))
                _relationMatrixes[citizenId] = relationMatrix = new CitizenRelation(citizenId);
            return relationMatrix;
        }

        /// <summary>
        /// Clears out all known relations of all citizens.
        /// </summary>
        public void Clear()
        {
            _lastDecayCheckMinute = 0;
            _relationMatrixes.Clear();
        }

        public IEnumerator<CitizenRelation> GetEnumerator()
        {
            return _relationMatrixes.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Add's or replaces the citizen relation for the given citizen.
        /// </summary>
        /// <param name="relation"></param>
        internal void AddOrReplace(CitizenRelation relation)
        {
            _relationMatrixes[relation.CitizenId] = relation;
        }

        internal void RaiseEvent(EventName eventName, EventArgs args)
        {
            switch(eventName)
            {
                case EventName.KnowChange:
                    OnKnowChanged?.Invoke(this, args as RelationChangeArgs);
                    break;
                case EventName.LikeChange:
                    OnLikeChanged?.Invoke(this, args as RelationChangeArgs);
                    break;
                case EventName.Seen:
                    OnPlayerSeen?.Invoke(this, args as SeenPlayerArgs);
                    break;
                default:
                    throw new NotSupportedException($"Event ({(int)eventName}) \"{eventName}\" not supported.");
            }
        }

        internal enum EventName
        {
            KnowChange,
            LikeChange,
            Seen
        }

        internal bool IsLoading { get; private set; }

        /// <summary>
        /// Loads all citizen relation information from a save file.
        /// </summary>
        /// <param name="e"></param>
        internal void Load(SaveGameArgs e)
        {
            IsLoading = true;
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
#pragma warning disable CS0618 // Type or member is obsolete
            var relationMatrixPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"RelationsPlusData_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            var newPath = Lib.SaveGame.MigrateOldSaveStructure(relationMatrixPath, e, "sod_relationsplus_RelationsPlusData.json");

            // Reset for a new load
            Clear();

            // Relation matrix loading
            if (File.Exists(newPath))
            {
                var relationMatrixJson = File.ReadAllText(newPath);
                var citizenSaveData = JsonSerializer.Deserialize<CitizenSaveData>(relationMatrixJson);

                // Set values
                _lastDecayCheckMinute = citizenSaveData.LastDecayCheckMinute;
                foreach (var citizenRelation in citizenSaveData.RelationMatrix)
                    _relationMatrixes.Add(citizenRelation.Key, citizenRelation.Value);

                Plugin.Log.LogInfo("Loaded citizen relations.");
            }
            IsLoading = false;
        }

        /// <summary>
        /// Saves all citizen relation information to a save file.
        /// </summary>
        /// <param name="e"></param>
        internal void Save(SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
#pragma warning disable CS0618 // Type or member is obsolete
            var relationMatrixPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"RelationsPlusData_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            var newPath = Lib.SaveGame.MigrateOldSaveStructure(relationMatrixPath, e, "sod_relationsplus_RelationsPlusData.json");

            if (!_relationMatrixes.Any())
            {
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                    Plugin.Log.LogInfo("Deleted citizen relations.");
                }
            }
            else
            {
                // Collect data to save
                var saveObject = new CitizenSaveData
                {
                    RelationMatrix = _relationMatrixes,
                    LastDecayCheckMinute = _lastDecayCheckMinute
                };

                // Serialize to json and save
                var citizenSaveDataJson = JsonSerializer.Serialize(saveObject, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(newPath, citizenSaveDataJson);
                Plugin.Log.LogInfo("Saved citizen relations.");
            }
        }

        internal static void Delete(SaveGameArgs e)
        {
            // Support migration, but handle deletion in SOD.Common
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
#pragma warning disable CS0618 // Type or member is obsolete
            var relationFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"RelationsPlusData_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            _ = Lib.SaveGame.MigrateOldSaveStructure(relationFilePath, e, "sod_relationsplus_RelationsPlusData.json");
        }

        /// <summary>
        /// Handles the automatic decay logic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void Timed_DecayLogic(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (_lastDecayCheckMinute >= Plugin.Instance.Config.DecayTimeMinutesCheck)
            {
                _lastDecayCheckMinute = 0;

                var decayKnowAmount = Plugin.Instance.Config.DecayKnowAmount;
                var decaySince = Plugin.Instance.Config.DecayKnowAfterUnseenMinutes;
                var canImproveToNeutral = Plugin.Instance.Config.AllowAutoImproveLikeToNeutral;
                var improveAmount = Plugin.Instance.Config.ImproveLikeAmount;
                var decayLikeAmount = Plugin.Instance.Config.DecayLikeAmount;
                var canDecayLike = Plugin.Instance.Config.AllowDecayLikeToNeutral;
                var debugMode = Plugin.Instance.Config.DebugMode;

                if (debugMode)
                    Plugin.Log.LogInfo("[Debug]: Start decay process.");

                // Decay each recorded citizen's know value automatically if they haven't seen the player for more than X minutes
                foreach (var citizen in _relationMatrixes.Values)
                {
                    if (citizen.LastSeenGameTime == null) continue;

                    // Decay process
                    if (citizen.LastSeenGameTime.Value.AddMinutes(decaySince) <= Lib.Time.CurrentDateTime)
                    {
                        // Take decay gates into account to not decay past these values once reached.
                        var currentDecayGate = GetCurrentKnowGate(citizen);

                        // Apply decay for 'Know'
                        citizen.Know = UnityEngine.Mathf.Clamp(citizen.Know + decayKnowAmount, currentDecayGate, 1f);

                        // Apply decay for 'Like'
                        if (canDecayLike && citizen.Like > 0.5f)
                            citizen.Like = UnityEngine.Mathf.Clamp(citizen.Like + decayLikeAmount, 0.5f, 1f);
                    }

                    // Like improvement process back to neutral, can happen every check
                    if (canImproveToNeutral && citizen.Like < 0.5f)
                    {
                        // Apply improvement
                        citizen.Like = UnityEngine.Mathf.Clamp(citizen.Like + improveAmount, 0f, 0.5f);
                    }
                }

                if (debugMode)
                    Plugin.Log.LogInfo("[Debug]: Finalized decay process.");
            }
            else
            {
                _lastDecayCheckMinute++;
            }
        }

        /// <summary>
        /// Returns the current know gate of the given relation.
        /// </summary>
        /// <param name="citizenRelation"></param>
        /// <returns></returns>
        private static float GetCurrentKnowGate(CitizenRelation citizenRelation)
        {
            // If we allow decay past gates, we are always at gate 0f
            if (Plugin.Instance.Config.AllowDecayPastKnowGates)
                return 0f;

            var know = citizenRelation.Know;
            if (know < Plugin.Instance.Config.KnowGateOne)
                return 0f;

            if (know < Plugin.Instance.Config.KnowGateTwo)
                return Plugin.Instance.Config.KnowGateOne;

            if (know < Plugin.Instance.Config.KnowGateThree)
                return Plugin.Instance.Config.KnowGateTwo;

            if (know < Plugin.Instance.Config.KnowGateFour)
                return Plugin.Instance.Config.KnowGateThree;

            return Plugin.Instance.Config.KnowGateFour;
        }
    }
}
