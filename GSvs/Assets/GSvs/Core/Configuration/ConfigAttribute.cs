using BepInEx.Configuration;
using GSvs.Core.Util;
using System;
using System.IO;
using System.Reflection;

namespace GSvs.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        public interface ICustomConfigImplementation
        {
            public ConfigEntryBase Bind(ConfigFile configFile, ConfigDefinition configDefinition, ConfigDescription configDescription);
        }

        public string section;
        public string key;
        public string description;

        public ConfigAttribute(string key = default)
        {
            this.key = key;
        }

        public void CompoundWith(ConfigAttribute other)
        {
            if (string.IsNullOrEmpty(section))
            {
                section = other.section;
            }
            if (string.IsNullOrEmpty(key))
            {
                key = other.key;
            }
            if (string.IsNullOrEmpty(description))
            {
                description = other.description;
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(key);
        }

        public static void Process(Type type)
        {
            ConfigAttribute typeConfig = type.GetCustomAttribute<ConfigAttribute>(false);
            if (typeConfig == null)
            {
                return;
            }
            string configPath = Path.Combine(type.Namespace.Split('.'));
            configPath = Path.ChangeExtension(configPath, "cfg");
            ConfigFile configFile = GSvsPlugin.GetConfigFile(configPath, false);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                ConfigAttribute fieldConfig = field.GetCustomAttribute<ConfigAttribute>(false);
                if (fieldConfig == null)
                {
                    continue;
                }
                fieldConfig.CompoundWith(typeConfig);
                if (!fieldConfig.IsValid())
                {
                    continue;
                }
                Type settingType = field.FieldType;
                object defaultValue = field.GetValue(null);

                ConfigDefinition configDefinition = new ConfigDefinition(fieldConfig.section, fieldConfig.key);
                ConfigDescription configDescription = string.IsNullOrEmpty(fieldConfig.description) 
                    ? null 
                    : new ConfigDescription(fieldConfig.description);

                if (typeof(ICustomConfigImplementation).IsAssignableFrom(settingType))
                {
                    if (defaultValue is ICustomConfigImplementation customConfigImplementation)
                    {
                        customConfigImplementation.Bind(configFile, configDefinition, configDescription);
                        if (settingType.IsValueType)
                        {
                            field.SetValue(null, customConfigImplementation);
                        }
                    }
                    continue;
                }

                ConfigEntryBase configEntry = configFile.Bind(settingType, configDefinition, defaultValue, configDescription);
                field.SetValue(null, configEntry.BoxedValue);
            }
        }
    }
}