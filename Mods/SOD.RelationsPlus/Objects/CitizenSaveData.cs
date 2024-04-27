using System.Collections.Generic;

namespace SOD.RelationsPlus.Objects
{
    /// <summary>
    /// Class is used to serialize the citizen data for saving
    /// </summary>
    public class CitizenSaveData
    {
        /// <summary>
        /// The last time the decay check was executed
        /// </summary>
        public int LastDecayCheckMinute { get; set; }
        /// <summary>
        /// The matrix of all the relations
        /// </summary>
        public Dictionary<int, CitizenRelation> RelationMatrix { get; set; }
    }
}
