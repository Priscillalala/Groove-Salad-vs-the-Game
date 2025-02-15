using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class BighornBison : ContentManipulator<BighornBison>
    {
        [InjectConfig(desc = "Give Bighorn Bison passive health regeneration to match the Bison Steak changes")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float BaseRegeneration = 2f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetRegen([LoadAsset("RoR2/Base/Bison/BisonBody.prefab")] GameObject BisonBody)
        {
            if (BisonBody.TryGetComponent(out CharacterBody characterBody))
            {
                characterBody.baseRegen = BaseRegeneration;
                characterBody.levelRegen = BaseRegeneration * 0.2f;
            }
        }
    }
}
