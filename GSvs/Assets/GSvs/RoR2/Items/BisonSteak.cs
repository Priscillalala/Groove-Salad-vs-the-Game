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
    // TODO: something cute when healing
    [HarmonyPatch]
    [LanguageOverrides]
    public abstract class BisonSteak : ContentManipulator<BisonSteak>
    {
        [InjectConfig(desc = "Bison Steak also increases healing")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly Percent 
            HealingIncrease = 0.08f,
            HealingIncreasePerStack = 0.08f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.Heal))]
        static void IncreaseHealing(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdflda<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.increaseHealing))
                );
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_3);
            c.EmitDelegate<Func<HealthComponent, float, bool, float>>((healthComponent, amount, nonRegen) =>
            {
                if (nonRegen)
                {
                    Inventory inventory = healthComponent.body.inventory;
                    if (inventory)
                    {
                        int itemCount = inventory.GetItemCount(RoR2Content.Items.FlatHealth);
                        if (itemCount > 0)
                        {
                            amount *= 1f + StackUtil.Scale(HealingIncrease, HealingIncreasePerStack, itemCount);
                        }
                    }
                }
                return amount;
            });
            c.Emit(OpCodes.Starg_S, 1);
        }
    }
}
