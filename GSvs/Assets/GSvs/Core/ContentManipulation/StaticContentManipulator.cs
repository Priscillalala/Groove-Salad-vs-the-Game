using BepInEx.Configuration;
using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;

namespace GSvs.Core.ContentManipulation
{
    public abstract class StaticContentManipulator<This> where This : StaticContentManipulator<This>
    {
        [Config("Installed")]
        public static bool installed;

        public static ConfigFile config;

        static StaticContentManipulator()
        {
            ContentManipulatorAttribute.InitImplementation += Init;
        }

        private static void Init()
        {
            if (installed)
            {
                OnInstall();
            }
        }

        private static void OnInstall()
        {
            GSvsPlugin.Harmony.CreateClassProcessor(typeof(This)).Patch();
            AssetManipulatorAttribute.ProcessAsync(typeof(This));
        }
    }
}