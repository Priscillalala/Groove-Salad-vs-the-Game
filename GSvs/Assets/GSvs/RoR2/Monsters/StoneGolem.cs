using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class StoneGolem : ContentManipulator<StoneGolem>
    {
        [InjectConfig(desc = "Reduce the cost of Stone Golems and also reduce their health to compensate")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly int Cost = 25;

        [InjectConfig]
        public static readonly float BaseMaxHealth = 380f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetCost(
            [LoadAsset("RoR2/Base/Golem/cscGolem.asset")] CharacterSpawnCard cscGolem,
            [LoadAsset("RoR2/Base/Golem/cscGolemNature.asset")] CharacterSpawnCard cscGolemNature,
            [LoadAsset("RoR2/Base/Golem/cscGolemSandy.asset")] CharacterSpawnCard cscGolemSandy,
            [LoadAsset("RoR2/Base/Golem/cscGolemSnowy.asset")] CharacterSpawnCard cscGolemSnowy
            )
        {
            cscGolem.directorCreditCost 
                = cscGolemNature.directorCreditCost 
                = cscGolemSandy.directorCreditCost 
                = cscGolemSnowy.directorCreditCost 
                = Cost;
        }

        [AssetManipulator]
        static void SetHealth([LoadAsset("RoR2/Base/Golem/GolemBody.prefab")] GameObject GolemBody)
        {
            if (GolemBody.TryGetComponent(out CharacterBody characterBody))
            {
                characterBody.baseMaxHealth = BaseMaxHealth;
                characterBody.levelMaxHealth = Mathf.Round(BaseMaxHealth * 0.3f);
            }
        }
    }
}
