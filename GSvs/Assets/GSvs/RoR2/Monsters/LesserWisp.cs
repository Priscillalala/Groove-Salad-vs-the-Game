using EntityStates.Wisp1Monster;
using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using GSvs.Core.Util;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class LesserWisp : ContentManipulator<LesserWisp>
    {
        [InjectConfig(desc = "Increase the health, chase speed, and damage of Lesser Wisps")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float BaseMaxHealth = 60f;

        [InjectConfig]
        public static readonly float ChaseNearbySpeedMultiplier = 2f;

        [InjectConfig]
        public static readonly float ChaseFarSpeedMultiplier = 3f;

        [InjectConfig]
        public static readonly float EmbersDamageCoefficient = 2f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetHealth([LoadAsset("RoR2/Base/Wisp/WispBody.prefab")] GameObject WispBody)
        {
            if (WispBody.TryGetComponent(out CharacterBody characterBody))
            {
                characterBody.baseMaxHealth = BaseMaxHealth;
                characterBody.levelMaxHealth = Mathf.Round(BaseMaxHealth * 0.3f);
            }
        }

        [AssetManipulator]
        static void BoostSpeed([LoadAsset("RoR2/Base/Wisp/WispMaster.prefab")] GameObject WispMaster)
        {
            AISkillDriver[] skillDrivers = WispMaster.GetComponents<AISkillDriver>();
            skillDrivers[2].moveInputScale = ChaseNearbySpeedMultiplier;
            skillDrivers[3].moveInputScale = ChaseFarSpeedMultiplier;
        }

        [AssetManipulator]
        static void SetDamage([LoadAsset("RoR2/Base/Wisp/EntityStates.Wisp1Monster.FireEmbers.asset")] EntityStateConfiguration FireEmbersConfiguration)
        {
            FireEmbersConfiguration.SetFieldValue(nameof(FireEmbers.damageCoefficient), EmbersDamageCoefficient);
        }
    }
}
