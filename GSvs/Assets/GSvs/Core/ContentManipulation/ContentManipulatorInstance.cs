using GSvs.Core.Util;
using HarmonyLib;

namespace GSvs.Core.ContentManipulation
{
    [ContentManipulator]
    public abstract class ContentManipulatorInstance
    {
        public PatchClassProcessor patcher;

        private bool enabled;

        public void SetEnabled(bool newEnabled)
        {
            if (enabled != newEnabled)
            {
                enabled = newEnabled;
                if (enabled)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }

        protected virtual void Enable()
        {
            patcher.Patch();
        }

        protected virtual void Disable()
        {
            patcher.Unpatch();
        }
    }
}