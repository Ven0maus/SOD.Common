using Il2CppInterop.Runtime;
using System.Collections.Generic;
using UnityEngine;
using static Il2CppSystem.Linq.Expressions.Interpreter.NullableMethodCallInstruction;

namespace SOD.Common.Extensions
{
    public static class GameExtensions
    {
        /// <summary>
        /// Determines if the actor sees the target actor.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Sees(this Actor actor, Actor target)
        {
            if (actor == null || target == null || actor.GetHashCode() == target.GetHashCode() ||
                actor.isDead || actor.isStunned || actor.isAsleep || !target.isSeenByOthers)
                return false;

            float distance = Vector3.Distance(actor.lookAtThisTransform.position, target.lookAtThisTransform.position);
            float maxDistance = Mathf.Min(GameplayControls.Instance.citizenSightRange, target.stealthDistance);
            if (distance <= maxDistance)
            {
                if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                    actor.ActorRaycastCheck(target, distance + 3f, out RaycastHit _, false, Color.green, Color.red, Color.white, 1f))
                {
                    return true;
                }
            }
            return false;
        }

        private static Il2CppSystem.Collections.Generic.Dictionary<string, ScriptableObject> GetResourceCacheCollection<T>(Toolbox toolbox)
        {
            var type = Il2CppType.Of<T>();
            if (!toolbox.resourcesCache.TryGetValue(type, out var dict))
            {
                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"[Debug]: GetFromResourceCache<{typeof(T).Name}> no resources found for the specified type.");
                return default;
            }
            return dict;
        }

        public static T GetFromResourceCache<T>(this Toolbox toolbox, string itemName)
            where T : ScriptableObject
        {
            var resourceCache = GetResourceCacheCollection<T>(toolbox);
            if (resourceCache == null) return default;

            if (!resourceCache.TryGetValue(itemName, out var scriptableObject))
            {
                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"[Debug]: GetFromResourceCache<{typeof(T).Name}>({itemName}) no resource found for the specified item name.");
                return default;
            }

            return scriptableObject.TryCast<T>();
        }

        public static IEnumerable<T> GetFromResourceCache<T>(this Toolbox toolbox)
            where T : ScriptableObject
        {
            var resourceCache = GetResourceCacheCollection<T>(toolbox);
            if (resourceCache == null) yield break;

            foreach (var value in resourceCache.Values)
                yield return value.TryCast<T>();
        }
    }
}
