using System;

namespace SOD.Common.BepInEx.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BindingAttribute : Attribute
    {
        /// <summary>
        /// The custom name of the property, if null it will take the default property name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The default initial value if no value has been determined yet.
        /// </summary>
        public object DefaultValue { get; }
        /// <summary>
        /// The description of the property.
        /// </summary>
        public string Description { get; }

        public BindingAttribute(object defaultValue, string description, string name = null)
        {
            Name = name;
            DefaultValue = defaultValue;
            Description = description;
        }
    }
}
