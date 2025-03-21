using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Mechanics
{
    public abstract class PostLoopBuffs : ContentManipulator<PostLoopBuffs>
    {
        [InjectConfig(desc = "")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            }
        }

        static void OnServerDamageDealt(DamageReport damageReport)
        {
            if (Run.instance 
                && Run.instance.loopClearCount > 0 
                && damageReport.victimTeamIndex == TeamIndex.Monster
                && damageReport.victim
                && damageReport.victim.itemCounts.adaptiveArmor == 0
                )
            {
                float adaptiveArmorPerHpPercent = damageReport.victimIsChampion ? 4f : 2f;
                float adaptiveArmorValue = damageReport.damageDealt / damageReport.victim.fullCombinedHealth * 100f * adaptiveArmorPerHpPercent;
                damageReport.victim.adaptiveArmorValue = Mathf.Min(damageReport.victim.adaptiveArmorValue + adaptiveArmorValue, 400f);
            }
        }
    }
}
