using GSvs.Core.Configuration;
using GSvs.Core.Util;
using GSvs.Events;
using RoR2;
using System.IO;

namespace GSvs.Core.ContentManipulation
{
    public abstract class ContentManipulatorInstance<This> : BaseContentManipulator<This> where This : ContentManipulatorInstance<This>
    {
        [Config(key = "Installed")]
        public static readonly RuntimeConfig<bool> installed = true;

        private static bool wasInstalled;

        protected override bool IsInstalled() => installed;

        public ContentManipulatorInstance() : base() 
        {
            wasInstalled = IsInstalled();
            installed.ValueChanged += UpdateInstallation;
        }

        private void UpdateInstallation()
        {
            bool isInstalled = IsInstalled();
            if (wasInstalled != isInstalled)
            {
                if (isInstalled)
                {
                    OnInstall();
                }
                else
                {
                    OnUninstall();
                }
                wasInstalled = isInstalled;
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
    }
}