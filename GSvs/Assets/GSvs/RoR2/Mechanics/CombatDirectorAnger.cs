using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace GSvs.RoR2.Mechanics
{
    public abstract class CombatDirectorAnger : ContentManipulator<CombatDirectorAnger>
    {
        [InjectConfig(desc = "Killing monsters angers combat directors over time, causing stronger monsters to spawn. Note that combat directors reset between stages")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            }
        }

        private static void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victim 
                && damageReport.victim.TryGetComponent(out DeathRewards deathRewards) 
                && deathRewards.spawnValue > 0 
                && damageReport.victimTeamIndex != TeamIndex.None
                )
            {
                float angerCoefficient = 1f + (deathRewards.spawnValue / 1000f);
                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    if (combatDirector.teamIndex == damageReport.victimTeamIndex)
                    {
                        combatDirector.creditMultiplier *= angerCoefficient;
                        combatDirector.expRewardCoefficient /= angerCoefficient;
                        foreach (var moneyWave in combatDirector.moneyWaves)
                        {
                            moneyWave.multiplier *= angerCoefficient;
                        }
                    }
                }
            }
        }
    }
}
