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

        /// <summary>
        /// The type of the defaultValue
        /// </summary>
        internal Type Type { get; }

        /// <summary>
        /// The base binding attribute, you can use the Type parameter to specify the type for the defaultValue (for example decimal, float) etc.
        /// <br>This type parameter is provided because of attribute parameter limitations.</br>
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="description"></param>
        /// <param name="name"></param>
        /// <param name="type">The defaultValue's real type</param>
        public BindingAttribute(object defaultValue, string description, string name = null, Type type = null)
        {
            Name = name;
            DefaultValue = defaultValue;
            Description = description;
        }
    }
}
