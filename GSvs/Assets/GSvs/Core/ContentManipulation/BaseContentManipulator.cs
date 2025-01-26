using GSvs.Core.Configuration;
using GSvs.Core.Util;
using GSvs.Events;
using HarmonyLib;
using RoR2;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;

namespace GSvs.Core.ContentManipulation
{
    public abstract class BaseContentManipulator<This> where This : BaseContentManipulator<This>
    {
        protected static PatchClassProcessor Patcher { get; private set; }
        protected static string LanguageOverridesRootFolder { get; private set; }

        public BaseContentManipulator()
        {
            ConfigAttribute.Process(typeof(This));
            Patcher = GSvsPlugin.Harmony.CreateClassProcessor(typeof(This));
            LanguageOverridesRootFolder = GetLanguageOverridesRootFolder();
            if (IsInstalled())
            {
                OnInstall();
            }
        }

        protected virtual string GetLanguageOverridesRootFolder()
        {
            string[] paths = typeof(This).FullName.Split('.');
            paths[0] = GSvsPlugin.RuntimeLanguageOverridesLocation;
            return Path.Combine(paths);
        }

        protected virtual bool IsInstalled() => true;

        protected virtual void OnInstall()
        {
            Patcher.Patch();
            if (!string.IsNullOrEmpty(LanguageOverridesRootFolder) && Directory.Exists(LanguageOverridesRootFolder))
            {
                LanguageEvents.CollectLanguageOverrideRootFolders += CollectLanguageOverrideRootFolders;
                if (Language.IsAnyLanguageLoaded())
                {
                    RoR2Application.instance.StartCoroutine(LanguageUtil.RebuildLanguagesCoroutine());
                }
            }
        }

        protected static void CollectLanguageOverrideRootFolders(List<string> list)
        {
            list.Add(LanguageOverridesRootFolder);
        }
    }
}