using SOD.Common;
using SOD.RelationsPlus.Objects;
using System;
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
    public sealed class RelationManager
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
        /// Custom indexer to get citizen relation information.
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

        /// <summary>
        /// Loads all citizen relation information from a save file.
        /// </summary>
        /// <param name="relationMatrixPath"></param>
        internal void Load(string hash)
        {
            var relationMatrixPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"CitizenRelations_{hash}.json");

            if (_relationMatrixes.Count > 0)
                _relationMatrixes.Clear();

            // Relation matrix loading
            if (File.Exists(relationMatrixPath))
            {
                var relationMatrixJson = File.ReadAllText(relationMatrixPath);
                var citizenRelations = JsonSerializer.Deserialize<Dictionary<int, CitizenRelation>>(relationMatrixJson);
                foreach (var citizenRelation in citizenRelations)
                    _relationMatrixes.Add(citizenRelation.Key, citizenRelation.Value);
            }

            Plugin.Log.LogInfo("Loaded citizen relations and player interests information.");
        }

        /// <summary>
        /// Saves all citizen relation information to a save file.
        /// </summary>
        /// <param name="filePath"></param>
        internal void Save(string hash)
        {
            var relationMatrixPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"CitizenRelations_{hash}.json");

            if (!_relationMatrixes.Any())
            {
                if (File.Exists(relationMatrixPath))
                    File.Delete(relationMatrixPath);
            }
            else
            {
                // Relation matrixes
                var relationMatrixJson = JsonSerializer.Serialize(_relationMatrixes, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(relationMatrixPath, relationMatrixJson);
            }

            Plugin.Log.LogInfo("Saved citizen relations and player interests information.");
        }

        internal static void Delete(string hash)
        {
            var relationFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"CitizenRelations_{hash}.json");

            if (File.Exists(relationFilePath))
                File.Delete(relationFilePath);

            Plugin.Log.LogInfo("Deleted citizen relations and player interests information.");
        }
    }
}
