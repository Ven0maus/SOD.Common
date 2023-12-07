using System;

namespace SOD.Common.BepInEx.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BindingAttribute : Attribute
    {
        public string Name { get; }
        public object DefaultValue { get; }
        public string Description { get; }

        public BindingAttribute(string name, object defaultValue, string description)
        {
            Name = name;
            DefaultValue = defaultValue;
            Description = description;
        }
    }
}
