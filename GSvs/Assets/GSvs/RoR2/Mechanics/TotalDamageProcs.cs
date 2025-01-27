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
    public abstract class TotalDamageProcs : ContentManipulator<TotalDamageProcs>
    {
        [InjectConfig(desc = "Use the old TOTAL damage formula from early access. Procs that deal more than 100% TOTAL damage (most of them) inherit less damage from attacks that deal more than 100% base damage")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Util), nameof(Util.OnHitProcDamage))]
        static void CapProcDamage(float damageThatProccedIt, float damageStat, float damageCoefficient, ref float __result)
        {
            float a = damageThatProccedIt + (damageCoefficient - 1f) * damageStat;
            __result = Mathf.Min(__result, a);
        }
    }
}
