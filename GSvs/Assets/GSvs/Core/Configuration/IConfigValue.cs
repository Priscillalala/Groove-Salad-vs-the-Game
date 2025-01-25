using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public interface IConfigValue
    {
        public object BoxedValue { get; }
        public bool ValueCanChange { get; }

        public event Action ValueChanged;

        public ConfigEntryBase Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription);

    }
}