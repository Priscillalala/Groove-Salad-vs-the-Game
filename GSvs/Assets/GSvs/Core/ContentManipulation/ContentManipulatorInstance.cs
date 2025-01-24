using GSvs.Core.Configuration;
using GSvs.Core.Util;
using HarmonyLib;

namespace GSvs.Core.ContentManipulation
{
    public abstract class ContentManipulatorInstance<This> where This : ContentManipulatorInstance<This>, new()
    {
        [Config("Installed")]
        public readonly static ConfigValue<bool> installed;

        protected PatchClassProcessor patcher;

        static ContentManipulatorInstance()
        {
            ContentManipulatorAttribute.InitImplementation += Init;
        }

        private static void Init()
        {
            This instance = new This
            {
                patcher = GSvsPlugin.Harmony.CreateClassProcessor(typeof(This))
            };
            if (installed)
            {
                instance.OnInstall();
            }
            installed.ValueChanged += instance.UpdateInstallation;
        }

        public void UpdateInstallation()
        {
            if (installed)
            {
                OnInstall();
            }
            else
            {
                OnUninstall();
            }
        }

        protected virtual void OnInstall()
        {
            patcher.Patch();
        }

        protected virtual void OnUninstall()
        {
            patcher.Unpatch();
        }
    }
}