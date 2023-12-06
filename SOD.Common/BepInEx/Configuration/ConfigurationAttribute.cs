using System;

namespace SOD.Common.BepInEx.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigurationAttribute : Attribute
    {
        public string Name { get; }
        public object DefaultValue { get; }
        public string Description { get; }

        public ConfigurationAttribute(string name, object defaultValue, string description)
        {
            Name = name;
            DefaultValue = defaultValue;
            Description = description;
        }
    }
}
