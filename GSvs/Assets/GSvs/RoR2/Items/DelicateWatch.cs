using GSvs.Core;
using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using GSvs.Core.Util;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;

namespace GSvs.RoR2.Items
{
    // TODO: finish buff icon and activation vfx
    [HarmonyPatch]
    [LanguageOverrides]
    [AssetManipulator]
    public abstract class DelicateWatch : ContentManipulator<DelicateWatch>
    {
        [InjectConfig(desc = "Rework Delicate Watch")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly Percent 
            AttackSpeedIncreasePerBuff = 0.025f,
            AttackSpeedIncreasePerBuffPerStack = 0.005f;

        [InjectConfig]
        public static readonly int
            MaxBuffs = 12,
            MaxBuffsPerStack = 6;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            }
        }

        static void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(DLC1Content.Items.FragileDamageBonus);
                if (itemCount > 0)
                {
                    int buffCount = sender.GetBuffCount(GSvsRoR2Content.Buffs.DelicateWatchBonus);
                    args.attackSpeedMultAdd += buffCount * StackUtil.Scale(AttackSpeedIncreasePerBuff, AttackSpeedIncreasePerBuffPerStack, itemCount);
                }
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(HealthComponent), nameof(HealthComponent.TakeDamageProcess))]
        static void RemoveDamageBonus(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.FragileDamageBonus)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount))
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(HealthComponent.ItemCounts), MethodType.Constructor, typeof(Inventory))]
        static void RemoveItemBreaking(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.fragileDamageBonus))
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
        }

        [AssetManipulator]
        static void UpdateItemTags([LoadAsset("RoR2/DLC1/FragileDamageBonus/FragileDamageBonus.asset")] ItemDef FragileDamageBonus)
        {
            FragileDamageBonus.tags = new[] { ItemTag.Damage };
        }
    }
}
