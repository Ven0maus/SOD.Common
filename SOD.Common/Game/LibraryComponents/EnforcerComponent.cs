using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SOD.Common.Game.LibraryComponents
{
    public sealed class EnforcerComponent
    {
        internal EnforcerComponent() { }

        /// <summary>
        /// The enforcer which is at the latest murder location.
        /// </summary>
        public GameplayController.EnforcerCall EnforcerAtLatestMurderLocation
        {
            get
            {
                var latestMurder = Plugin.Helpers.Murders.LatestActiveMurder;
                return latestMurder == null || !GameplayController.Instance.enforcerCalls.TryGetValue(latestMurder.location, out var enforcer) ?
                    null : enforcer;
            }
        }

        /// <summary>
        /// All enforcers
        /// </summary>
        public Il2CppSystem.Collections.Generic.List<Human> Enforcers => GameplayController.Instance.enforcers;
    }
}
