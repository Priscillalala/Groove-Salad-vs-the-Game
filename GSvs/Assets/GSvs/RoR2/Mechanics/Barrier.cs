using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace GSvs.RoR2.Mechanics
{
    [HarmonyPatch]
    public abstract class Barrier : ContentManipulator<Barrier>
    {
        [InjectConfig(desc = "Barrier decay is dynamic, like Risk of Rain Returns (slower at low barrier, faster at high barrier)")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.ServerFixedUpdate))]
        static void DynamicBarrierDecay(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchCallOrCallvirt(AccessTools.PropertyGetter(typeof(CharacterBody), nameof(CharacterBody.barrierDecayRate)))
                );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>((barrierDecayRate, healthComponent) =>
            {
                float minBarrierDecayRate = barrierDecayRate / healthComponent.fullBarrier * 100f;
                float maxBarrierDecayRate = barrierDecayRate * 2f;
                return Mathf.Lerp(minBarrierDecayRate, maxBarrierDecayRate, healthComponent.barrier / healthComponent.fullBarrier);
            });
        }
    }
}
