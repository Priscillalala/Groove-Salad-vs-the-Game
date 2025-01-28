using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSvs.RoR2.Interactables
{
    [AssetManipulator]
    public abstract class Scrapper : ContentManipulator<Scrapper>
    {
        [InjectConfig(desc = "Scrappers provide a limited selection of items")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void ModifyScrapper([LoadAsset("RoR2/Base/Scrapper/Scrapper.prefab")] GameObject Scrapper)
        {
            GSvsPlugin.Logger.LogMessage("Modify Scrapper");
            Scrapper.AddComponent<ModifedScrapper>();
        }
    }
}
