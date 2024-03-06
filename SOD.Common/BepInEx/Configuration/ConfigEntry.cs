using BepInEx.Configuration;
using System;

namespace SOD.Common.BepInEx.Configuration
{
    /// <summary>
    /// Non generic config entry for BepInEx ConfigFile.
    /// </summary>
    public sealed class ConfigEntry : ConfigEntryBase
    {
        private object _value;

        public override object BoxedValue
        {
            get
            {
                return _value;
            }
            set
            {
                value = ClampValue(value);
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnSettingChanged(this);
                }
            }
        }

        /// <summary>
        /// Fired when the setting is changed. Does not detect changes made outside from this object.
        /// </summary>
        public event EventHandler SettingChanged;

        public ConfigEntry(ConfigFile configFile, ConfigDefinition definition, Type settingType, object defaultValue, ConfigDescription configDescription)
            : base(configFile, definition, settingType, defaultValue, configDescription)
        {
            configFile.SettingChanged += delegate (object sender, SettingChangedEventArgs args)
            {
                if (args.ChangedSetting == this)
                {
                    SettingChanged?.Invoke(sender, args);
                }
            };
        }
    }
}
