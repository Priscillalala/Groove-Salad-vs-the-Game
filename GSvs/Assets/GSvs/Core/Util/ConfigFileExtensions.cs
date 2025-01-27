using BepInEx.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace GSvs.Core.Util
{
    public static class ConfigFileExtensions
    {
        private static readonly MethodInfo _Bind = typeof(ConfigFile)
            .GetMember(nameof(ConfigFile.Bind), MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public)
            .Cast<MethodInfo>()
            .First(BindFunctionPredicate);

        private static bool BindFunctionPredicate(MethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            return parameters[0].ParameterType == typeof(ConfigDefinition) && parameters[2].ParameterType == typeof(ConfigDescription);
        }

        public static ConfigEntryBase Bind(this ConfigFile configFile, Type settingType, ConfigDefinition configDefinition, object defaultValue, ConfigDescription configDescription = null)
        {
            MethodInfo genericBind = _Bind.MakeGenericMethod(settingType);
            return (ConfigEntryBase)genericBind.Invoke(configFile, new object[] { configDefinition, defaultValue, configDescription });
        }
    }
}