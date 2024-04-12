using System;
using System.Collections.Generic;

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

        }

        /// <summary>
        /// Saves all citizen relation information to a save file.
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath)
        {

        }
    }
}
