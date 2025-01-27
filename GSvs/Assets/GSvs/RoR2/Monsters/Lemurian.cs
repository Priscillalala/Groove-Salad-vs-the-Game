using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class Lemurian : ContentManipulator<Lemurian>
    {
        [InjectConfig(desc = "Increase the health of Lemurians")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float BaseMaxHealth = 144f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetHealth([LoadAsset("RoR2/Base/Lemurian/LemurianBody.prefab")] GameObject LemurianBody)
        {
            if (LemurianBody.TryGetComponent(out CharacterBody characterBody))
            {
                characterBody.baseMaxHealth = BaseMaxHealth;
                characterBody.levelMaxHealth = Mathf.Round(BaseMaxHealth * 0.3f);
            }
        }
    }
}
