using GSvs.Core;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace GSvs.RoR2.Mechanics
{
    [HarmonyPatch]
    [LanguageOverrides]
    public abstract class Bleed : ContentManipulator<Bleed>
    {
        [InjectConfig(desc = "Bleed deals TOTAL damage instead of base damage. Affects Tri-Tip Dagger, Shatterspleen, and Noxious Thorn")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly Percent TotalDamageCoefficient = 1.2f;

        [InjectConfig(desc = "Bleed duration at 100% damage. Higher damage attacks bleed for longer")]
        public static readonly float BaseDuration = 2f;

        private static float compensatedDamageMultiplier;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        } 

        [SystemInitializer(typeof(DotController))]
        static void InitAfterDotController()
        {
            if (Installed)
            {
                var bleedDef = DotController.GetDotDef(DotController.DotIndex.Bleed);
                float idealDamageCoefficient = TotalDamageCoefficient / BaseDuration * bleedDef.interval;
                compensatedDamageMultiplier = idealDamageCoefficient / bleedDef.damageCoefficient;
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(GlobalEventManager), nameof(GlobalEventManager.ProcessHitEnemy))]
        static void TotalDamageBleed(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locAttackerBodyIndex = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(out locAttackerBodyIndex),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_bleedChance")
                );
            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<DotController>(nameof(DotController.InflictDot))
                );
            c.GotoPrev(MoveType.After,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.procCoefficient)),
                x => x.MatchMul(),
                x => x.MatchLdcR4(out _)
                );
            // Ignore original duration and damage multiplier
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloc, locAttackerBodyIndex);
            c.EmitDelegate<Func<DamageInfo, CharacterBody, float>>((DamageInfo damageInfo, CharacterBody attackerBody) =>
            {
                float procDamage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, TotalDamageCoefficient);
                float procDamageCoefficient = procDamage / attackerBody.damage;
                float bleedMult = Mathf.Sqrt(procDamageCoefficient / TotalDamageCoefficient);
                return BaseDuration * bleedMult;
            });
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Func<float, float>>(duration =>
            {
                float bleedMult = duration / BaseDuration;
                return compensatedDamageMultiplier * bleedMult;
            });
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(DotController), nameof(DotController.AddDot))]
        static void ConsistentBleedResetting(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel[] switchLabels = null;
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(3),
                x => x.MatchSwitch(out switchLabels)
                );
            ILLabel bleedLabel = switchLabels[(int)DotController.DotIndex.Bleed];
            c.GotoLabel(bleedLabel, MoveType.AfterLabel);
            int locDotStackListIndex = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<DotController>(nameof(DotController.dotStackList)),
                x => x.MatchLdloc(out locDotStackListIndex)
                //x => x.MatchCallOrCallvirt<List<DotController.DotStack>>("get_Item")
                );

            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<DotController.DotStack>(nameof(DotController.DotStack.timer))
                );
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_3);
            c.Emit(OpCodes.Ldloc, locDotStackListIndex);
            c.EmitDelegate<Func<float, DotController, DotController.DotIndex, int, float>>(CapBleedDuration);

            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<DotController.DotStack>(nameof(DotController.DotStack.totalDuration))
                );
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_3);
            c.Emit(OpCodes.Ldloc, locDotStackListIndex);
            c.EmitDelegate<Func<float, DotController, DotController.DotIndex, int, float>>(CapBleedDuration);

            static float CapBleedDuration(float duration, DotController dotController, DotController.DotIndex dotIndex, int dotStackListIndex)
            {
                if (dotIndex == DotController.DotIndex.Bleed)
                {
                    return Mathf.Min(duration, dotController.dotStackList[dotStackListIndex].totalDuration);
                }
                return duration;
            }
        }
    }
}
