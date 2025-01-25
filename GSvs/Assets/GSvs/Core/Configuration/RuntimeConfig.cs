using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public struct RuntimeConfig<T> : IConfigValue
    {
        private readonly T defaultValue;
        private ConfigEntry<T> entry;

        public readonly T Value => entry != null ? entry.Value : defaultValue;
        readonly object IConfigValue.BoxedValue => Value;
        readonly bool IConfigValue.ValueCanChange => true;

        public event Action ValueChanged;

        public RuntimeConfig(T defaultValue)
        {
            this.defaultValue = defaultValue;
            entry = null;
            ValueChanged = null;
        }

        ConfigEntryBase IConfigValue.Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription)
        {
            entry = configFile.Bind(configDefinition, defaultValue, configDescription);
            if (!Equals(Value, defaultValue))
            {
                ValueChanged?.Invoke();
            }
            entry.SettingChanged += OnSourceSettingChanged;
            return entry;
        }

        private readonly void OnSourceSettingChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke();
        }

        public static implicit operator RuntimeConfig<T>(T defaultValue) => new RuntimeConfig<T>(defaultValue);

        public static implicit operator T(RuntimeConfig<T> configValue) => configValue.Value;
    }
}