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
    public abstract class DifficultyScaling : ContentManipulator<DifficultyScaling>
    {
        [InjectConfig(desc = "")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(Run), nameof(Run.RecalculateDifficultyCoefficentInternal))]
        static void SetScalingValues(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdfld<DifficultyDef>(nameof(DifficultyDef.scalingValue)),
                x => x.MatchMul()
                ))
            {
                GSvsPlugin.Logger.LogWarning("Set linear scaling mult");
                c.Next.Operand = 0.06f;
            }
            c.Index = 0;
            while (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Run>(nameof(Run.stageClearCount)),
                x => x.MatchConvR4(),
                x => x.MatchCallOrCallvirt<Mathf>(nameof(Mathf.Pow))
                ))
            {
                GSvsPlugin.Logger.LogWarning("Set stage scaling mult");
                c.Next.Operand = 1.1f;
                c.Index += 5;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Run, float>>(run => Mathf.Pow(1.35f, run.loopClearCount));
                c.Emit(OpCodes.Mul);
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(Run), nameof(Run.GetDifficultyScaledCost), typeof(int), typeof(float))]
        static void LoopedPriceScaling(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Mathf>(nameof(Mathf.Pow)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Run, float>>(run => Mathf.Pow(1.35f, run.loopClearCount));
            c.Emit(OpCodes.Mul);
        }
    }
}
