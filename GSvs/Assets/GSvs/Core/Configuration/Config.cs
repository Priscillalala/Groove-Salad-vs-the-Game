using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public struct Config<T> : IConfigValue
    {
        public T Value { get; private set; }
        readonly object IConfigValue.BoxedValue => Value;
        readonly bool IConfigValue.ValueCanChange => false;

        readonly event Action IConfigValue.ValueChanged
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public Config(T defaultValue)
        {
            Value = defaultValue;
        }

        ConfigEntryBase IConfigValue.Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription)
        {
            ConfigEntry<T> entry = configFile.Bind(configDefinition, Value, configDescription);
            Value = entry.Value;
            return entry;
        }

        public static implicit operator Config<T>(T defaultValue) => new Config<T>(defaultValue);

        public static implicit operator T(Config<T> configValue) => configValue.Value;
    }
}