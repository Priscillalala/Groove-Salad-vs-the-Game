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
using Object = UnityEngine.Object;

namespace GSvs.RoR2.Mechanics
{
    [HarmonyPatch]
    public abstract class TeleportOut : ContentManipulator<TeleportOut>
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

        [HarmonyPostfix, HarmonyPatch(typeof(ConvertPlayerMoneyToExperience), MethodType.Constructor)]
        static void SetBurstInvertal(ConvertPlayerMoneyToExperience __instance)
        {
            __instance.burstInterval = 0.125f;
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(ConvertPlayerMoneyToExperience), nameof(ConvertPlayerMoneyToExperience.FixedUpdate))]
        static void SetDestroyDelay(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Object>(nameof(Object.Destroy))
                );
            c.GotoPrev(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ConvertPlayerMoneyToExperience>(nameof(ConvertPlayerMoneyToExperience.burstTimer)),
                x => x.MatchLdcR4(out _),
                x => x.MatchBgeUn(out _)
                );
            c.Index += 2;
            c.Next.Operand = -1f;
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(TeleportOutController), MethodType.Constructor)]
        static void SetSFXDelay(TeleportOutController __instance)
        {
            __instance.delayBeforePlayingSFX = 0f;
        }

        //[HarmonyILManipulator, HarmonyPatch(typeof(TeleportOutController), nameof(TeleportOutController.FixedUpdate))]
        static void SetWarmupDuration(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.Before, 
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<TeleportOutController>(nameof(TeleportOutController.fixedAge)),
                x => x.MatchLdcR4(out _),
                x => x.MatchBltUn(out _)
                );
            c.Index += 2;
            c.Next.Operand = 1f;
        }
    }
}
