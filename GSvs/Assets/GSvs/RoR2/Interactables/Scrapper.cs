using GSvs.Core;
using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSvs.RoR2.Interactables
{
    [AssetManipulator]
    [LanguageOverrides]
    [HarmonyPatch]
    public abstract class Scrapper : ContentManipulator<Scrapper>
    {
        [InjectConfig(desc = "Scrappers only include items collected on the current stage")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float SelectionWeightMultiplier = 1.5f;

        [InjectConfig(desc = "-1 is infinite")]
        public static readonly int MaxSpawnsPerStage = 2;

        private static InteractableSpawnCard cachedIscScrapper;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                Inventory.onServerItemGiven += OnServerItemGiven;
                DirectorCardCategorySelection.calcCardWeight += CalcCardWeight;
            }
        }

        static void OnServerItemGiven(Inventory inventory, ItemIndex itemIndex, int count)
        {
            if (count > 0)
            {
                if (!inventory.TryGetComponent(out ScrapperInventory scrapperInventory))
                {
                    scrapperInventory = inventory.gameObject.AddComponent<ScrapperInventory>();
                }
                scrapperInventory.OnItemAdded(itemIndex, count);
            }
        }

        static void CalcCardWeight(DirectorCard card, ref float weight)
        {
            if (card.spawnCard && card.spawnCard == cachedIscScrapper)
            {
                weight *= SelectionWeightMultiplier;
            }
        }

        [AssetManipulator]
        static void ModifyScrapper([LoadAsset("RoR2/Base/Scrapper/Scrapper.prefab")] GameObject Scrapper)
        {
            Scrapper.AddComponent<ModifedScrapper>();
        }

        [AssetManipulator]
        static void ModifyScrapperSpawnCard([LoadAsset("RoR2/Base/Scrapper/iscScrapper.asset")] InteractableSpawnCard iscScrapper)
        {
            iscScrapper.maxSpawnsPerStage = MaxSpawnsPerStage;
            cachedIscScrapper = iscScrapper;
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(ScrapperController), nameof(ScrapperController.BeginScrapping))]
        static void ScrapCollectedItems(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locInteractorBodyIndex = -1;
            int locPickupDefIndex = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(out locInteractorBodyIndex),
                x => x.MatchCallOrCallvirt(AccessTools.PropertyGetter(typeof(CharacterBody), nameof(CharacterBody.inventory))),
                x => x.MatchLdloc(out locPickupDefIndex),
                x => x.MatchLdfld<PickupDef>(nameof(PickupDef.itemIndex)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount))
                );
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Mathf>(nameof(Mathf.Min)));
            c.Emit(OpCodes.Ldloc, locInteractorBodyIndex);
            c.Emit(OpCodes.Ldloc, locPickupDefIndex);
            c.EmitDelegate<Func<int, CharacterBody, PickupDef, int>>((scrappingItemsCount, interactorBody, pickupDef) =>
            {
                if (interactorBody.inventory.TryGetComponent(out ScrapperInventory scrapperInventory))
                {
                    ItemIndex itemIndex = pickupDef.itemIndex;
                    int collectedItemsCount = scrapperInventory.collectedItemCounts.GetValueOrDefault(itemIndex);
                    if (scrappingItemsCount < collectedItemsCount)
                    {
                        scrapperInventory.collectedItemCounts[itemIndex] -= scrappingItemsCount;
                        return scrappingItemsCount;
                    }
                    scrapperInventory.collectedItemCounts.Remove(itemIndex);
                    return collectedItemsCount;
                }
                return 0;
            });
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(ScrapperInfoPanelHelper), nameof(ScrapperInfoPanelHelper.AddQuantityToPickerButton))]
        static void DisplayCollectedItemQuantity(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)));
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate<Func<int, ScrapperInfoPanelHelper, PickupDef, int>>((itemCount, scrapperInfoPanelHelper, pickupDef) =>
            {
                if (scrapperInfoPanelHelper.cachedBodyInventory.TryGetComponent(out ScrapperInventory scrapperInventory))
                {
                    return Mathf.Min(itemCount, scrapperInventory.collectedItemCounts.GetValueOrDefault(pickupDef.itemIndex));
                }
                return 0;
            });
        }
    }
}
