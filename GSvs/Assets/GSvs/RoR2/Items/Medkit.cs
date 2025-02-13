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
    public abstract class Medkit : ContentManipulator<Medkit>
    {
        [InjectConfig(desc = "Remove the fractional healing from Medkit")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float 
            Healing = 20f,
            HealingPerStack = 20f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.RemoveBuff), typeof(BuffIndex))]
        static void SetHealing(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locItemCount = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.Medkit)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out locItemCount)
                );
            int locProcChainMaskIndex = -1;
            c.GotoNext(MoveType.Before,
                x => x.MatchLdloca(out locProcChainMaskIndex),
                x => x.MatchInitobj<ProcChainMask>(),
                x => x.MatchLdloc(locProcChainMaskIndex),
                x => x.MatchLdcI4(out _),
                x => x.MatchCallOrCallvirt<HealthComponent>(nameof(HealthComponent.Heal))
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldloc, locItemCount);
            c.EmitDelegate<Func<int, float>>(itemCount => StackUtil.Scale(Healing, HealingPerStack, itemCount));
        }
    }
}
