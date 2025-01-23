using BepInEx.Configuration;
using System;
using System.Reflection;

namespace GSvs.Core.Util
{
    public static class ConfigFileExtensions
    {
        private static readonly MethodInfo _Bind = typeof(ConfigFile).GetMethod(nameof(ConfigFile.Bind));

        public static ConfigEntryBase Bind(this ConfigFile configFile, Type settingType, ConfigDefinition configDefinition, object defaultValue, ConfigDescription configDescription = null)
        {
            MethodInfo genericBind = _Bind.MakeGenericMethod(settingType);
            return (ConfigEntryBase)genericBind.Invoke(configFile, new object[] { configDefinition, defaultValue, configDescription });
        }
    }
}