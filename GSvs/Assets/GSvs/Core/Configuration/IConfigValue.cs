using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public interface IConfigValue
    {
        public object BoxedValue { get; }
        public ConfigValueUpdater ValueUpdater { get; }

        public void Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription, ConfigValueUpdater configValueUpdater);
    }
}