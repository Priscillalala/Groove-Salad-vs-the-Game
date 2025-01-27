using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class Beetle : ContentManipulator<Beetle>
    {
        [InjectConfig(desc = "Remove Beetles from the game; RIP")]
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
        static void RemoveBeetles([LoadAsset("RoR2/Base/Beetle/cscBeetle.asset")] CharacterSpawnCard cscBeetle)
        {
            cscBeetle.prefab = null;
        }
    }
}
