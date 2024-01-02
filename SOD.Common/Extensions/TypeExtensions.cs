using System;
using System.Linq;
using UniverseLib;

namespace SOD.Common.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsAssignableFrom(this Il2CppSystem.Object obj, Type otherType)
        {
            var baseTypes = ReflectionUtility.GetAllBaseTypes(obj);
            Plugin.Log.LogInfo("All base types: " + string.Join(",", baseTypes.Select(a => a.Name)));
            return baseTypes.Any(a => a == otherType);
        }
    }
}
