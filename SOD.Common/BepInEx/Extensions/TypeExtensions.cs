using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.BepInEx.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Expand interfaces hierarchy of the given interface
        /// </summary>
        /// <param name="type">The most derived interface</param>
        /// <returns>List of inherited interfaces including the given one in the following order: most derived goes last</returns>
        public static IEnumerable<Type> ExpandInheritedInterfaces(this Type type)
        {
            var inheritedInterfaces = type.GetInterfaces().ToList();
            return inheritedInterfaces
                .OrderBy(baseInterface => inheritedInterfaces.Count(inheritedInterface => inheritedInterface.IsAssignableFrom(baseInterface)))
                .Append(type);
        }

        /// <summary>
        /// Expand interfaces hierarchy of the given list of interfaces
        /// </summary>
        /// <param name="interfaces">List of the most derived interface</param>
        /// <returns>List of inherited interfaces including the given ones in the following order: most derived goes last</returns>
        public static IEnumerable<Type> ExpandInheritedInterfaces(this IEnumerable<Type> interfaces)
        {
            return interfaces.SelectMany(i => i.ExpandInheritedInterfaces());
        }
    }

}
