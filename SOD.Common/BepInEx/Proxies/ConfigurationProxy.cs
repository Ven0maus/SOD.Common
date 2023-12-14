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
                    throw new Exception($"Missing Binding attribute for interface configuration property \"{info.Name}\".");
                if (configurationAttribute.Name.Trim() == string.Empty)
                    throw new Exception("Cannot provide an empty or whitespace value for Binding Name. Leave empty for auto generation or specify a valid name.");

                property.GetMethod = builder =>
                {
                    var name = configurationAttribute.Name ?? $"General.{info.Name}";
                    if (!builder.ExistsInternal(name, out var entryBase))
                    {
                        SetByType(info, type, builder, configurationAttribute, configurationAttribute.DefaultValue);
                        builder.ExistsInternal(name, out entryBase);
                        return entryBase.BoxedValue;
                    }
                    return entryBase.BoxedValue;
                };
                property.SetMethod = (builder, value) =>
                {
                    SetByType(info, type, builder, configurationAttribute, value);
                };
            });
        }

        private static void SetByType(PropertyInfo info, Type type, ConfigBuilder builder, BindingAttribute configurationAttribute, object value)
        {
            var name = configurationAttribute.Name ?? $"General.{info.Name}";

            // Supported types by BepInEx config file:
            /* string, bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, enum */
            if (type == typeof(string))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (string)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (string)value);
            }
            else if (type == typeof(bool))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (bool)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (bool)value);
            }
            else if (type == typeof(byte))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (byte)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (byte)value);
            }
            else if (type == typeof(sbyte))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (sbyte)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (sbyte)value);
            }
            else if (type == typeof(short))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (short)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (short)value);
            }
            else if (type == typeof(ushort))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (ushort)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (ushort)value);
            }
            else if (type == typeof(int))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (int)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (int)value);
            }
            else if (type == typeof(uint))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (uint)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (uint)value);
            }
            else if (type == typeof(long))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (long)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (long)value);
            }
            else if (type == typeof(ulong))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (ulong)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (ulong)value);
            }
            else if (type == typeof(float))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (float)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (float)value);
            }
            else if (type == typeof(double))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (double)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (double)value);
            }
            else if (type == typeof(decimal))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (decimal)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (decimal)value);
            }
            else if (type.IsEnum)
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (Enum)configurationAttribute.DefaultValue);
                else
                    builder.Set(name, (Enum)value);
            }
        }
    }
}
