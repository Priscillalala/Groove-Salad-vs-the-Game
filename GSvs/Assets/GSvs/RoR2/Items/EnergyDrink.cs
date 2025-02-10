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
    public abstract class EnergyDrink : ContentManipulator<EnergyDrink>
    {
        [InjectConfig(desc = "Reduce the stacking power and slightly reduce the base power of Energy Drink")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly Percent 
            SprintSpeedIncrease = 0.24f,
            SprintSpeedIncreasePerStack = 0.12f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.RecalculateStats))]
        static void SetSprintSpeedIncrease(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locItemCount = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.SprintBonus)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out locItemCount)
                );
            c.GotoNext(MoveType.After,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(locItemCount),
                x => x.MatchConvR4(),
                x => x.MatchMul()
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldloc, locItemCount);
            c.EmitDelegate<Func<int, float>>(itemCount => StackUtil.Scale(SprintSpeedIncrease, SprintSpeedIncreasePerStack, itemCount));
        }
    }
}
