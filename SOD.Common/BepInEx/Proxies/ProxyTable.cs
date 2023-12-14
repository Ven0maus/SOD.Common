using Castle.DynamicProxy;
using SOD.Common.BepInEx.Configuration;
using SOD.Common.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SOD.Common.BepInEx.Proxies
{
    /// <summary>
    /// Accessor lookup table for proxy implementations
    /// </summary>
    /// <typeparam name="TInterface">Proxied interface</typeparam>
    /// <typeparam name="TImpl">Implementation type</typeparam>
    internal static class ProxyTable<TInterface>
        where TInterface : class
    {
        private static Dictionary<string, Func<ConfigBuilder, object>> _getters;
        private static Dictionary<string, Action<ConfigBuilder, object>> _setters;

        /// <summary>
        /// Interface property information
        /// </summary>
        internal class Property
        {
            /// <summary>
            /// Property information (reflection)
            /// </summary>
            internal PropertyInfo PropertyInfo { get; }

            internal Property(PropertyInfo info)
            {
                PropertyInfo = info;
            }

            /// <summary>
            /// Property get delegate
            /// </summary>
            internal Func<ConfigBuilder, object> GetMethod
            {
                set => AddGetMethod(PropertyInfo, value);
            }

            /// <summary>
            /// Property set delegate
            /// </summary>
            internal Action<ConfigBuilder, object> SetMethod
            {
                set => AddSetMethod(PropertyInfo, value);
            }
        }

        /// <summary>
        /// Build the accessor tables
        /// </summary>
        /// <param name="rebuild">Rebuild existing tables</param>
        /// <param name="generator">Accessor generator (invoked for each property of the interface)</param>
        internal static void Build(bool rebuild, Action<Property> generator)
        {
            // Initialize tables (quick exit if already present)
            if (_setters != null && _getters != null && !rebuild)
            {
                return;
            }
            _setters = new Dictionary<string, Action<ConfigBuilder, object>>();
            _getters = new Dictionary<string, Func<ConfigBuilder, object>>();

            // Create accessors for all properties defined on the interface
            // Include properties from inherited interfaces
            foreach (Type i in typeof(TInterface).ExpandInheritedInterfaces())
            {
                GenerateAccessors(i, generator);
            }
        }

        /// <summary>
        /// Create accessors for all properties defined on a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generator"></param>
        private static void GenerateAccessors(Type type, Action<Property> generator)
        {
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                generator.Invoke(new Property(property));
            }
        }

        /// <summary>
        /// Add a getter to the accessor tables
        /// </summary>
        /// <param name="property">Property information</param>
        /// <param name="getter">Property get delegate</param>
        private static void AddGetMethod(PropertyInfo property, Func<ConfigBuilder, object> getter)
        {
            MethodInfo method = property.GetGetMethod();
            if (method == null)
            {
                return;
            }
            _getters[method.Name] = getter;
        }

        /// <summary>
        /// Add a setter to the accessor tables
        /// </summary>
        /// <param name="property">Property information</param>
        /// <param name="setter">Property set delegate</param>
        private static void AddSetMethod(PropertyInfo property, Action<ConfigBuilder, object> setter)
        {
            MethodInfo method = property.GetSetMethod();
            if (method == null) return;
            _setters[method.Name] = setter;
        }

        /// <summary>
        /// Invoke a proxy method
        /// </summary>
        /// <param name="invocation">Proxy method (getter/setter)</param>
        /// <param name="impl">Implementation object</param>
        /// <returns>Proxy return message</returns>
        internal static void Invoke(IInvocation invocation, ConfigBuilder impl)
        {
            string method = invocation.Method.Name;
            if (method.StartsWith("get_"))
            {
                // Find getter delegate for the property
                Func<ConfigBuilder, object> getter = _getters[method];
                // Invoke delegate to retrieve property value
                invocation.ReturnValue = getter.Invoke(impl);
                return;
            }
            if (method.StartsWith("set_"))
            {
                // Find setter delegate for the property
                Action<ConfigBuilder, object> setter = _setters[method];
                // Invoke delegate to set property value
                object value = invocation.Arguments[0];
                setter.Invoke(impl, value);
                return;
            }
            throw new NotSupportedException();
        }
    }
}
