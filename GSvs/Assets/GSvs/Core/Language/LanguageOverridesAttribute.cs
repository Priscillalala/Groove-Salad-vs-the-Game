using GSvs.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GSvs.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LanguageOverridesAttribute : Attribute
    {
        private static List<string> languageOverridesRootFolders;

        public static void Process(Type type)
        {
            LanguageOverridesAttribute languageOverridesAttribute = type.GetCustomAttribute<LanguageOverridesAttribute>();
            if (languageOverridesAttribute == null)
            {
                return;
            }
            if (languageOverridesRootFolders == null)
            {
                languageOverridesRootFolders = new List<string>();
                LanguageEvents.CollectLanguageOverrideRootFolders += CollectLanguageOverrideRootFolders;
            }
            languageOverridesRootFolders.Add(GetDefaultLanguageOverridesRootFolder(type));
        }

        private static void CollectLanguageOverrideRootFolders(List<string> list)
        {
            list.AddRange(languageOverridesRootFolders);
        }

        private static string GetDefaultLanguageOverridesRootFolder(Type type)
        {
            string[] paths = type.FullName.Split('.');
            paths[0] = GSvsPlugin.RuntimeLanguageOverridesLocation;
            return Path.Combine(paths);
        }
    }
}