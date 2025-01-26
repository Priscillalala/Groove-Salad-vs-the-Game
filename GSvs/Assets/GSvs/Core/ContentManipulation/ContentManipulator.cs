using GSvs.Core.AssetManipulation;
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
    public abstract class ContentManipulator<This> where This : ContentManipulator<This>
    {
        [Config("Installed")]
        public static readonly ConfigValue<bool> installed = true;

        public static PatchClassProcessor Patcher { get; private set; }
        public static string LanguageOverridesRootFolder { get; private set; }
        public static bool WasInstalled { get; private set; }

        public ContentManipulator()
        {
            ConfigAttribute.Process(typeof(This));
            Patcher = GSvsPlugin.Harmony.CreateClassProcessor(typeof(This));
            LanguageOverridesRootFolder = GetLanguageOverridesRootFolder();
            bool isInstalled = IsInstalled();
            if (isInstalled)
            {
                OnInstall();
            }
            WasInstalled = isInstalled;
            if (installed.ValueUpdater != null)
            {
                installed.ValueUpdater.OnValueUpdated += UpdateInstallation;
            }
        }

        protected virtual string GetLanguageOverridesRootFolder()
        {
            string[] paths = typeof(This).FullName.Split('.');
            paths[0] = GSvsPlugin.RuntimeLanguageOverridesLocation;
            return Path.Combine(paths);
        }

        protected virtual bool IsInstalled() => installed;

        private void UpdateInstallation()
        {
            bool isInstalled = IsInstalled();
            if (WasInstalled != isInstalled)
            {
                if (isInstalled)
                {
                    OnInstall();
                }
                else
                {
                    OnUninstall();
                }
                WasInstalled = isInstalled;
            }
        }

        protected virtual void OnInstall()
        {
            Patcher.Patch();
            AssetManipulatorAttribute.ProcessAsync(typeof(This));
            if (!string.IsNullOrEmpty(LanguageOverridesRootFolder) && Directory.Exists(LanguageOverridesRootFolder))
            {
                LanguageEvents.CollectLanguageOverrideRootFolders += CollectLanguageOverrideRootFolders;
                if (Language.IsAnyLanguageLoaded())
                {
                    RoR2Application.instance.StartCoroutine(LanguageUtil.RebuildLanguagesCoroutine());
                }
            }
        }

        protected virtual void OnUninstall()
        {
            Patcher.Unpatch();
            if (!string.IsNullOrEmpty(LanguageOverridesRootFolder) && Directory.Exists(LanguageOverridesRootFolder))
            {
                LanguageEvents.CollectLanguageOverrideRootFolders -= CollectLanguageOverrideRootFolders;
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