using Castle.DynamicProxy;
using SOD.Common.BepInEx.Configuration;
using System;
using System.Reflection;

namespace SOD.Common.BepInEx.Proxies
{
    internal sealed class ConfigurationProxy<T> : IInterceptor
        where T : class
    {
        private static readonly ProxyGenerator _proxyGenerator;
        private readonly ConfigBuilder _configBuilder;

        static ConfigurationProxy()
        {
            _proxyGenerator = new ProxyGenerator();
        }

        internal ConfigurationProxy(ConfigBuilder builder)
        {
            _configBuilder = builder;
        }

        internal static T Create(ConfigBuilder builder)
        {
            var proxy = new ConfigurationProxy<T>(builder);
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(proxy);
        }

        public void Intercept(IInvocation invocation)
        {
            BuildProxyTable();
            ProxyTable<T>.Invoke(invocation, _configBuilder);
        }

        private static void BuildProxyTable(bool rebuild = false)
        {
            ProxyTable<T>.Build(rebuild, property =>
            {
                PropertyInfo info = property.PropertyInfo;
                Type type = info.PropertyType;

                var configurationAttribute = info.GetCustomAttribute<BindingAttribute>() ??
                    throw new Exception($"Invalid property \"{info.Name}\" defined without a configuration attribute.");

                property.GetMethod = builder =>
                {
                    if (!builder.ExistsInternal(configurationAttribute.Name, out var entryBase))
                    {
                        SetByType(type, builder, configurationAttribute, configurationAttribute.DefaultValue);
                        builder.ExistsInternal(configurationAttribute.Name, out entryBase);
                        return entryBase.BoxedValue;
                    }
                    return entryBase.BoxedValue;
                };
                property.SetMethod = (builder, value) =>
                {
                    SetByType(type, builder, configurationAttribute, value);
                };
            });
        }

        private static void SetByType(Type type, ConfigBuilder builder, BindingAttribute configurationAttribute, object value)
        {
            // Supported types by BepInEx config file:
            /* string, bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, enum */
            if (type == typeof(string))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (string)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (string)value);
            }
            else if (type == typeof(bool))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (bool)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (bool)value);
            }
            else if (type == typeof(byte))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (byte)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (byte)value);
            }
            else if (type == typeof(sbyte))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (sbyte)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (sbyte)value);
            }
            else if (type == typeof(short))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (short)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (short)value);
            }
            else if (type == typeof(ushort))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (ushort)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (ushort)value);
            }
            else if (type == typeof(int))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (int)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (int)value);
            }
            else if (type == typeof(uint))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (uint)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (uint)value);
            }
            else if (type == typeof(long))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (long)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (long)value);
            }
            else if (type == typeof(ulong))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (ulong)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (ulong)value);
            }
            else if (type == typeof(float))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (float)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (float)value);
            }
            else if (type == typeof(double))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (double)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (double)value);
            }
            else if (type == typeof(decimal))
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (decimal)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (decimal)value);
            }
            else if (type.IsEnum)
            {
                if (!builder.ExistsInternal(configurationAttribute.Name, out _))
                    builder.Add(configurationAttribute.Name, configurationAttribute.Description, (Enum)configurationAttribute.DefaultValue);
                else
                    builder.Set(configurationAttribute.Name, (Enum)value);
            }
        }
    }
}
