using System;
using System.Linq;
using UnityEngine.Playables;
using UniverseLib;

namespace SOD.Common.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determine if the IL2CPP object is assignable from the given type.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this Il2CppSystem.Object obj, Type otherType)
        {
            var baseTypes = ReflectionUtility.GetAllBaseTypes(obj);
            return baseTypes.Any(a => a == otherType);
        }

        /// <summary>
        /// Return's a deterministic hash code that is always the same for a given value, even between processes.
        /// <br>Uses FNV hashing algorithm.</br>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetFnvHashCode(this string value)
        {
            return (int)Lib.SaveGame.GetUniqueNumber(value);
        }
    }
}
