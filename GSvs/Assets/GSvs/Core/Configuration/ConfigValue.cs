using BepInEx.Configuration;

namespace GSvs.Core.Configuration
{
    public class ConfigValue<T> : IConfigValue
    {
        public T Value { get; private set; }
        object IConfigValue.BoxedValue => Value;
        public ConfigEntry<T> Entry { get; private set; }
        public ConfigValueUpdater ValueUpdater { get; private set; }

        public ConfigValue(T defaultValue)
        {
            Value = defaultValue;
            Entry = null;
            ValueUpdater = null;
        }

        void IConfigValue.Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription, ConfigValueUpdater configValueUpdater)
        {
            Entry = configFile.Bind(configDefinition, Value, configDescription);
            UpdateValue();
            if (configValueUpdater != null)
            {
                configValueUpdater.Bind(Entry);
                configValueUpdater.OnValueUpdated += UpdateValue;
                ValueUpdater = configValueUpdater;
            }
        }

        private void UpdateValue()
        {
            Value = Entry.Value;
        }

        public static implicit operator ConfigValue<T>(T defaultValue) => new ConfigValue<T>(defaultValue);

        public static implicit operator T(ConfigValue<T> configValue) => configValue.Value;
    }
}