using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
        /// <param name="filePath"></param>
        public void Load(string filePath)
        {
            if (_relationMatrixes.Count > 0)
                _relationMatrixes.Clear();

            var json = File.ReadAllText(filePath);
            var citizenRelations = JsonSerializer.Deserialize<Dictionary<int, CitizenRelation>>(json);
            foreach (var citizenRelation in citizenRelations)
                _relationMatrixes.Add(citizenRelation.Key, citizenRelation.Value);
        }

        /// <summary>
        /// Saves all citizen relation information to a save file.
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath)
        {
            if (!_relationMatrixes.Any())
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return;
            }

            var json = JsonSerializer.Serialize(_relationMatrixes, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(filePath, json);
        }
    }
}
