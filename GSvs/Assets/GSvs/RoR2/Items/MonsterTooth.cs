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
    public abstract class MonsterTooth : ContentManipulator<MonsterTooth>
    {
        [InjectConfig(desc = "Remove the fractional healing from Monster Tooth")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float 
            Healing = 8f,
            HealingPerStack = 8f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(GlobalEventManager), nameof(GlobalEventManager.OnCharacterDeath))]
        static void SetHealing(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locItemCount = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.Tooth)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out locItemCount)
                );
            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<HealthPickup>(nameof(HealthPickup.flatHealing))
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldloc, locItemCount);
            c.EmitDelegate<Func<int, float>>(itemCount => StackUtil.Scale(Healing, HealingPerStack, itemCount));
            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<HealthPickup>(nameof(HealthPickup.fractionalHealing))
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_R4, 0f);
        }
    }
}
