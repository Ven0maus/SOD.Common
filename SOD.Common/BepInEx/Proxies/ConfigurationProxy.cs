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

                var name = string.IsNullOrWhiteSpace(configurationAttribute.Name) ? $"General.{info.Name}" : configurationAttribute.Name;
                property.GetMethod = builder =>
                {
                    if (!builder.ExistsInternal(name, out var entryBase))
                    {
                        SetByType(type, builder, configurationAttribute, name, configurationAttribute.DefaultValue);
                        builder.ExistsInternal(name, out entryBase);
                        return entryBase.BoxedValue;
                    }
                    return entryBase.BoxedValue;
                };
                property.SetMethod = (builder, value) =>
                {
                    SetByType(type, builder, configurationAttribute, name, value);
                };
            });
        }

        private static void SetByType(Type type, ConfigBuilder builder, BindingAttribute configurationAttribute, string name, object value)
        {
            // Supported types by BepInEx config file:
            /* string, bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, enum */
            if (type == typeof(string))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (string)TryConvert(configurationAttribute.DefaultValue, typeof(string)));
                else
                    builder.Set(name, (string)value);
            }
            else if (type == typeof(bool))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (bool)TryConvert(configurationAttribute.DefaultValue, typeof(bool)));
                else
                    builder.Set(name, (bool)value);
            }
            else if (type == typeof(byte))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (byte)TryConvert(configurationAttribute.DefaultValue, typeof(byte)));
                else
                    builder.Set(name, (byte)value);
            }
            else if (type == typeof(sbyte))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (sbyte)TryConvert(configurationAttribute.DefaultValue, typeof(sbyte)));
                else
                    builder.Set(name, (sbyte)value);
            }
            else if (type == typeof(short))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (short)TryConvert(configurationAttribute.DefaultValue, typeof(short)));
                else
                    builder.Set(name, (short)value);
            }
            else if (type == typeof(ushort))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (ushort)TryConvert(configurationAttribute.DefaultValue, typeof(ushort)));
                else
                    builder.Set(name, (ushort)value);
            }
            else if (type == typeof(int))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (int)TryConvert(configurationAttribute.DefaultValue, typeof(int)));
                else
                    builder.Set(name, (int)value);
            }
            else if (type == typeof(uint))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (uint)TryConvert(configurationAttribute.DefaultValue, typeof(uint)));
                else
                    builder.Set(name, (uint)value);
            }
            else if (type == typeof(long))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (long)TryConvert(configurationAttribute.DefaultValue, typeof(long)));
                else
                    builder.Set(name, (long)value);
            }
            else if (type == typeof(ulong))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (ulong)TryConvert(configurationAttribute.DefaultValue, typeof(ulong)));
                else
                    builder.Set(name, (ulong)value);
            }
            else if (type == typeof(float))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (float)TryConvert(configurationAttribute.DefaultValue, typeof(float)));
                else
                    builder.Set(name, (float)value);
            }
            else if (type == typeof(double))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (double)TryConvert(configurationAttribute.DefaultValue, typeof(double)));
                else
                    builder.Set(name, (double)value);
            }
            else if (type == typeof(decimal))
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(name, configurationAttribute.Description, (decimal)TryConvert(configurationAttribute.DefaultValue, typeof(decimal)));
                else
                    builder.Set(name, (decimal)value);
            }
            else if (type.IsEnum)
            {
                if (!builder.ExistsInternal(name, out _))
                    builder.Add(TryConvert(configurationAttribute.DefaultValue, type), name, configurationAttribute.Description);
                else
                    builder.Set(value, name);
            }
            else
            {
                throw new Exception($"Configuration type {type.Name} is not supported.");
            }
        }

        private static object TryConvert(object obj, Type type)
        {
            try
            {
                return Convert.ChangeType(obj, type);
            }
            catch (InvalidCastException)
            {
                throw new Exception($"The value \"{obj}\" cannot be converted to the provided type \"{type.Name}\".");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
