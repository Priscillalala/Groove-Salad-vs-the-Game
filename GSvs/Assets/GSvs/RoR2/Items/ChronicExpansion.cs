using AK.Wwise;
using GSvs.Core;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    [HarmonyPatch]
    [LanguageOverrides]
    public abstract class ChronicExpansion : ContentManipulator<ChronicExpansion>
    {
        [InjectConfig(desc = "Nerf Chronic Expansion, rename it Tally Counter, and only beep at max stacks")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly Percent 
            DamageIncreasePerKill = 0.025f,
            DamageIncreasePerKillPerStack = 0.005f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.RecalculateStats))]
        static void SetDamageIncrease(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locItemCount = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.IncreaseDamageOnMultiKill)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out locItemCount)
                );
            int locStackIncrease = -1;
            c.GotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(locItemCount),
                x => x.MatchLdcI4(out _),
                x => x.MatchSub(),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchStloc(out locStackIncrease)
                );
            c.Next.Operand = DamageIncreasePerKillPerStack.Value;
            c.GotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(locStackIncrease),
                x => x.MatchAdd()
                );
            c.Next.Operand = DamageIncreasePerKill.Value;
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(IncreaseDamageOnMultiKillItemDisplayUpdater), nameof(IncreaseDamageOnMultiKillItemDisplayUpdater.SetVisibleHologram))]
        static void BeepAtMaxStacks(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<IncreaseDamageOnMultiKillItemDisplayUpdater>(nameof(IncreaseDamageOnMultiKillItemDisplayUpdater.BlinkAwayEvent)),
                x => x.MatchCallOrCallvirt(AccessTools.PropertyGetter(typeof(BaseType), nameof(BaseType.Id))),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt(AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject))),
                x => x.MatchCallOrCallvirt<AkSoundEngine>(nameof(AkSoundEngine.PostEvent))
                );
            ILLabel breakLabel = null;
            c.GotoPrev(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchBrfalse(out breakLabel)
                );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<IncreaseDamageOnMultiKillItemDisplayUpdater, bool>>(updater => updater.hasHitMaxBuff);
            c.Emit(OpCodes.Brfalse, breakLabel);
        }
    }
}
