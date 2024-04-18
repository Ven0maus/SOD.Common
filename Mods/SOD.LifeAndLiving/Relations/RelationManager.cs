using SOD.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using UnityEngine;

namespace SOD.LifeAndLiving.Relations
{
    /// <summary>
    /// Contains methods to create, update and delete relations between citizens and load/save the relation state per savegame.
    /// </summary>
    internal sealed class RelationManager
    {
        private static RelationManager _instance;
        /// <summary>
        /// The instance accessor for the RelationManager class
        /// </summary>
        internal static RelationManager Instance => _instance ??= new RelationManager();

        /// <summary>
        /// A relation matrix dictionary for each unique citizen.
        /// </summary>
        private readonly Dictionary<int, CitizenRelation> _relationMatrixes = new();

        /// <summary>
        /// Custom indexer to Get/Set citizen relations.
        /// </summary>
        /// <param name="citizenId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public CitizenRelation this[int citizenId]
        {
            get { return Get(citizenId); }
            set 
            { 
                Set(citizenId, (value as CrInstruction)?.Instruction ?? 
                throw new Exception("Setter argument must be a CrInstruction.")); 
            }
        }

        public PlayerInterests PlayerInterests { get; private set; } = new PlayerInterests();

        /// <summary>
        /// Determines if the relation manager is loading serialized data.
        /// </summary>
        public bool IsLoading { get; internal set; } = false;

        private RelationManager()
        { }

        /// <summary>
        /// Updates relation information of a citizen.
        /// </summary>
        /// <param name="citizenId"></param>
        /// <param name="info"></param>
        public void Set(int citizenId, Action<CitizenRelation> info)
        {
            if (!_relationMatrixes.TryGetValue(citizenId, out var relationMatrix))
                _relationMatrixes[citizenId] = relationMatrix = new CitizenRelation();
            info.Invoke(relationMatrix);
        }

        public CitizenRelation Get(int citizenId)
        {
            if (!_relationMatrixes.TryGetValue(citizenId, out var relationMatrix))
                _relationMatrixes[citizenId] = relationMatrix = new CitizenRelation();
            return relationMatrix;
        }

        /// <summary>
        /// Delete's relation information of a citizen.
        /// </summary>
        /// <param name="citizenId"></param>
        public void Delete(int citizenId)
        {
            _relationMatrixes.Remove(citizenId);
        }

        /// <summary>
        /// Loads all citizen relation information from a save file.
        /// </summary>
        /// <param name="relationMatrixPath"></param>
        public void Load(string hash)
        {
            IsLoading = true;

            var relationMatrixPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"CitizenRelations_{hash}.json");
            var playerInterestsPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"PlayerInterests_{hash}.json");

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

            // Player interests loading
            if (File.Exists(playerInterestsPath))
            {
                var playerInterestsJson = File.ReadAllText(playerInterestsPath);
                PlayerInterests = JsonSerializer.Deserialize<PlayerInterests>(playerInterestsJson);
            }

            IsLoading = false;

            Plugin.Log.LogInfo("Loaded citizen relations and player interests information.");
        }

        /// <summary>
        /// Saves all citizen relation information to a save file.
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string hash)
        {
            var relationMatrixPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"CitizenRelations_{hash}.json");
            var playerInterestsPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"PlayerInterests_{hash}.json");

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

            if (!PlayerInterests.ContainsContent)
            {
                if (File.Exists(playerInterestsPath))
                    File.Delete(playerInterestsPath);
            }
            else
            {
                // Player interests
                var playerInterestsJson = JsonSerializer.Serialize(PlayerInterests, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(playerInterestsPath, playerInterestsJson);
            }

            Plugin.Log.LogInfo("Saved citizen relations and player interests information.");
        }

        public void Delete(string hash)
        {
            var relationFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"CitizenRelations_{hash}.json");
            var playerInterestsPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"PlayerInterests_{hash}.json");

            if (File.Exists(relationFilePath))
                File.Delete(relationFilePath);

            if (File.Exists(playerInterestsPath))
                File.Delete(playerInterestsPath);

            Plugin.Log.LogInfo("Deleted citizen relations and player interests information.");
        }
    }
}
