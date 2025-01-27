using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class Jellyfish : ContentManipulator<Jellyfish>
    {
        [InjectConfig(desc = "Increase the health of Jellyfish")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float BaseMaxHealth = 80f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetHealth([LoadAsset("RoR2/Base/Jellyfish/JellyfishBody.prefab")] GameObject JellyfishBody)
        {
            if (JellyfishBody.TryGetComponent(out CharacterBody characterBody))
            {
                characterBody.baseMaxHealth = BaseMaxHealth;
                characterBody.levelMaxHealth = Mathf.Round(BaseMaxHealth * 0.3f);
            }
        }
    }
}
