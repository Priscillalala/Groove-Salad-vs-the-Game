using BepInEx.Configuration;
using GSvs.Core.Util;
using HG.Reflection;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using BepInEx;

namespace GSvs.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectConfigAttribute : SearchableAttribute
    {
        public string file;
        public string section;
        public string key;
        public string desc;

        public FieldInfo TargetField => (FieldInfo)target;

        public ConfigFile GetConfigFile()
        {
            string configPath = file;
            if (string.IsNullOrEmpty(configPath))
            {
                /*string[] paths = TargetField.DeclaringType.Namespace.Split('.');
                paths[0] = "Groove_Salad vs. the Game";
                for (int i = 1; i < paths.Length; i++)
                {
                    paths[i] = NicifyUtil.NicifyIdentifier(paths[i]);
                }*/
                configPath = Path.ChangeExtension(Path.Combine(TargetField.DeclaringType.Namespace.Split('.')), "cfg");
            }
            return GSvsPlugin.GetConfigFile(configPath, false);
        }

        public ConfigDefinition GetConfigDefinition()
        {
            return new ConfigDefinition(section ?? NicifyUtil.NicifyIdentifier(TargetField.DeclaringType.Name), key ?? NicifyUtil.NicifyIdentifier(TargetField.Name));
        }

        public ConfigDescription GetConfigDescription()
        {
            if (desc == null)
            {
                return null;
            }
            return new ConfigDescription(desc);
        }

        public static void InjectAll()
        {
            GSvsPlugin.Logger.LogMessage("Inject config init");
            List<InjectConfigAttribute> injectConfigAttributes = new List<InjectConfigAttribute>();
            GetInstances(injectConfigAttributes);
            foreach (InjectConfigAttribute injectConfigAttribute in injectConfigAttributes)
            {
                FieldInfo targetField = injectConfigAttribute.TargetField;
                ConfigFile configFile = injectConfigAttribute.GetConfigFile();
                ConfigDefinition configDefinition = injectConfigAttribute.GetConfigDefinition();
                object defaultValue = targetField.GetValue(null);
                ConfigDescription configDescription = injectConfigAttribute.GetConfigDescription();
                ConfigEntryBase configEntry = configFile.Bind(targetField.FieldType, configDefinition, defaultValue, configDescription);
                targetField.SetValue(null, configEntry.BoxedValue);
            }
        }
    }
}