using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public struct ConfigValue<T> : ConfigAttribute.ICustomConfigImplementation, IValueWrapper<T>
    {
        private readonly T defaultValue;
        private ConfigEntry<T> source;

        public readonly T Value => source != null ? source.Value : defaultValue;

        public event Action ValueChanged;

        public ConfigValue(T defaultValue)
        {
            this.defaultValue = defaultValue;
            source = null;
            ValueChanged = null;
        }

        public ConfigEntryBase Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription)
        {
            source = configFile.Bind(configDefinition, defaultValue, configDescription);
            if (!Equals(Value, defaultValue))
            {
                ValueChanged?.Invoke();
            }
            source.SettingChanged += OnSourceSettingChanged;
            return source;
        }

        private readonly void OnSourceSettingChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke();
        }

        public static implicit operator ConfigValue<T>(T defaultValue) => new ConfigValue<T>(defaultValue);

        public static implicit operator T(ConfigValue<T> configValue) => configValue.Value;
    }
}