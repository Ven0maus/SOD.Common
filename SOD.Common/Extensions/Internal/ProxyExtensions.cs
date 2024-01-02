using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Extensions.Internal
{
    internal static class ProxyExtensions
    {
        /// <summary>
        /// Expand interfaces hierarchy of the given interface
        /// </summary>
        /// <param name="type">The most derived interface</param>
        /// <returns>List of inherited interfaces including the given one in the following order: most derived goes last</returns>
        internal static IEnumerable<Type> ExpandInheritedInterfaces(this Type type)
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
        internal static IEnumerable<Type> ExpandInheritedInterfaces(this IEnumerable<Type> interfaces)
        {
            return interfaces.SelectMany(i => i.ExpandInheritedInterfaces());
        }
    }
}
