using GSvs.Core.Configuration;
using GSvs.Core.Util;
using GSvs.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace GSvs.Core.ContentManipulation
{
    public abstract class BaseContentManipulator<This> where This : BaseContentManipulator<This>
    {
        protected static PatchClassProcessor patcher;
        protected static string languageOverridesRootFolder;

        public BaseContentManipulator()
        {
            ConfigAttribute.Process(typeof(This));
            patcher = GSvsPlugin.Harmony.CreateClassProcessor(typeof(This));
            SetLanguageOverridesFolder();
            if (IsInstalled())
            {
                OnInstall();
            }
        }

        public virtual void SetLanguageOverridesFolder()
        {

        }

        public virtual bool IsInstalled() => true;

        protected virtual void OnInstall()
        {
            patcher.Patch();
            if (!string.IsNullOrEmpty(languageOverridesRootFolder))
            {
                LanguageEvents.CollectLanguageOverrideRootFolders += CollectLanguageOverrideRootFolders;
            }
        }

        protected static void CollectLanguageOverrideRootFolders(List<string> list)
        {
            list.Add(languageOverridesRootFolder);
        }
    }
}