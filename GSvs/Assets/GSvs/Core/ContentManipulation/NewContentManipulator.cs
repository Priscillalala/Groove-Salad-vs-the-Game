using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.Language;
using GSvs.Core.Util;
using GSvs.Events;
using HarmonyLib;
using RoR2;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;

namespace GSvs.Core.ContentManipulation
{
    public abstract class NewContentManipulator<This> where This : NewContentManipulator<This>
    {
        protected static void DefaultInit()
        {
            GSvsPlugin.Harmony.CreateClassProcessor(typeof(This)).Patch();
            AssetManipulatorAttribute.ProcessAsync(typeof(This));
            LanguageOverridesAttribute.Process(typeof(This));
        }
    }
}