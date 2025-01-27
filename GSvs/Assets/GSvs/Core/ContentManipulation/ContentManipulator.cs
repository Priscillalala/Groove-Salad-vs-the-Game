using GSvs.Core.AssetManipulation;

namespace GSvs.Core.ContentManipulation
{
    public abstract class ContentManipulator<This> where This : ContentManipulator<This>
    {
        protected static void DefaultInit()
        {
            GSvsPlugin.Harmony.CreateClassProcessor(typeof(This)).Patch();
            AssetManipulatorAttribute.ProcessAsync(typeof(This));
            LanguageOverridesAttribute.Process(typeof(This));
        }
    }
}