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
    [HarmonyPatch]
    public abstract class CombatDirectors : ContentManipulator<CombatDirectors>
    {
        [InjectConfig(desc = "Stage combat directors spawn more monsters at baseline")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float SpawningMultiplier = 1.4f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CombatDirector), nameof(CombatDirector.Awake))]
        static void ApplySpawningMultiplier(CombatDirector __instance)
        {
            if (__instance.GetComponent<DirectorCore>())
            {
                __instance.creditMultiplier *= SpawningMultiplier;
                __instance.maxSeriesSpawnInterval /= SpawningMultiplier;
                __instance.minRerollSpawnInterval /= SpawningMultiplier;
                __instance.maxRerollSpawnInterval /= SpawningMultiplier;
                __instance.expRewardCoefficient /= SpawningMultiplier;
            }
        }
    }
}
