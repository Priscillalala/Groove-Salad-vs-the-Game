using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace GSvs.RoR2.Mechanics
{
    [HarmonyPatch]
    public abstract class MonsterSpeed : ContentManipulator<MonsterSpeed>
    {
        [InjectConfig(desc = "Elite monsters move faster")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            }
        }

        static void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int boostSpeedCount = sender.inventory.GetItemCount(GSvsRoR2Content.Items.BoostSpeed);
                if (boostSpeedCount > 0)
                {
                    args.moveSpeedMultAdd += 0.1f * boostSpeedCount;
                }
            }    
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(CombatDirector), nameof(CombatDirector.Spawn))]
        static void EliteSpeedBoost(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<DirectorSpawnRequest>(nameof(DirectorSpawnRequest.onSpawnedServer))
                );
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate<Func<Action<SpawnCard.SpawnResult>, EliteDef, Action<SpawnCard.SpawnResult>>>((onSpawnedServer, eliteDef) =>
            {
                if (eliteDef)
                {
                    onSpawnedServer += OnEliteSpawnedServer;
                }
                return onSpawnedServer;

                void OnEliteSpawnedServer(SpawnCard.SpawnResult result)
                {
                    if (result.success && result.spawnedInstance && result.spawnedInstance.TryGetComponent(out CharacterMaster characterMaster))
                    {
                        int speedBoostCount = Mathf.Min((int)eliteDef.healthBoostCoefficient - 1, 5);
                        characterMaster.inventory.GiveItem(GSvsRoR2Content.Items.BoostSpeed, speedBoostCount);
                    }
                }
            });
        }
    }
}
