using GSvs.Core.Configuration;
using GSvs.Core.Util;
using HarmonyLib;

namespace GSvs.Core.ContentManipulation
{
    public abstract class ContentManipulatorInstance<This> where This : ContentManipulatorInstance<This>
    {
        [Config("Enabled")]
        public readonly static ConfigValue<bool> enabled;

        protected PatchClassProcessor patcher;

        public ContentManipulatorInstance()
        {
            patcher = GSvsPlugin.Harmony.CreateClassProcessor(typeof(This));
            if (enabled)
            {
                Install();
            }
            enabled.ValueChanged += UpdateEnabled;
        }

        public void UpdateEnabled()
        {
            if (enabled)
            {
                Install();
            }
            else
            {
                Uninstall();
            }
        }

        protected virtual void Install()
        {
            patcher.Patch();
        }

        protected virtual void Uninstall()
        {
            patcher.Unpatch();
        }
    }
}