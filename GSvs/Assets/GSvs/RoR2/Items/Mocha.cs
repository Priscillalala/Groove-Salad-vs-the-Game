using GSvs.Core;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using GSvs.Core.Util;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace GSvs.RoR2.Items
{
    [HarmonyPatch]
    [LanguageOverrides]
    public abstract class Mocha : ContentManipulator<Mocha>
    {
        [InjectConfig(desc = "Reduce the stacking power of Mocha movement speed to match Paul's Goat Hoof")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly Percent 
            MovementSpeedIncrease = 0.07f,
            MovementSpeedIncreasePerStack = 0.035f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.RecalculateStats))]
        static void SetMovementSpeedIncrease(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locItemCount = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.AttackSpeedAndMoveSpeed)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out locItemCount)
                );
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(locItemCount),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul()
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldloc, locItemCount);
            c.EmitDelegate<Func<int, float>>(itemCount => StackUtil.Scale(MovementSpeedIncrease, MovementSpeedIncreasePerStack, itemCount));
        }
    }
}
