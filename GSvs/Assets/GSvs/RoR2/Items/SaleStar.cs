using GSvs.Core;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace GSvs.RoR2.Items
{
    [ContentManipulator]
    [Config(section = "Sale Star Rework")]
    [HarmonyPatch]
    public class SaleStar : ContentManipulatorInstance<SaleStar>
    {
        [Config("Affected Chests Count")]
        public static ConfigValue<int> affectedChestsCount = 2;
        [Config("Affected Chests Count Per Stack")]
        public static ConfigValue<int> affectedChestsCountPerStack = 1;

        protected override void OnInstall()
        {
            SceneDirector.onPostPopulateSceneServer += OnPostPopulateSceneServer;
            base.OnInstall();
        }

        protected override void OnUninstall()
        {
            base.OnUninstall();
            SceneDirector.onPostPopulateSceneServer -= OnPostPopulateSceneServer;
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(InteractionDriver), nameof(InteractionDriver.MyFixedUpdate))]
        [HarmonyPatch(typeof(PurchaseInteraction), nameof(PurchaseInteraction.OnInteractionBegin))]
        private static void IgnoreSaleStar(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.LowerPricedChests)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount))
                );
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
        }

        private void OnPostPopulateSceneServer(SceneDirector sceneDirector)
        {
            if (!SceneInfo.instance.countsAsStage && !SceneInfo.instance.sceneDef.allowItemsToSpawnObjects)
            {
                return;
            }
            var rng = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
            int saleStarCount = CharacterMaster.readOnlyInstancesList.Sum(x => x.inventory.GetItemCount(DLC2Content.Items.LowerPricedChests));
            Debug.Log($"Sale star count: {saleStarCount}");
            if (saleStarCount <= 0)
            {
                return;
            }
            var purchaseInteractionInstances = InstanceTracker.GetInstancesList<PurchaseInteraction>();
            if (purchaseInteractionInstances == null)
            {
                return;
            }
            Debug.Log($"P interaction count: {purchaseInteractionInstances.Count}");
            List<PurchaseInteraction> shuffledPurchaseInteractionInstances = new List<PurchaseInteraction>(purchaseInteractionInstances);
            Util.ShuffleList(shuffledPurchaseInteractionInstances, rng);
            int remainingAffectedChestsCount = affectedChestsCount + (saleStarCount - 1) * affectedChestsCountPerStack;
            foreach (PurchaseInteraction purchaseInteraction in shuffledPurchaseInteractionInstances)
            {
                if (!purchaseInteraction.saleStarCompatible)
                {
                    continue;
                }

                if (purchaseInteraction.TryGetComponent(out ChestBehavior chestBehavior))
                {
                    chestBehavior.dropCount++;
                    chestBehavior.minDropCount++;
                    chestBehavior.maxDropCount++;
                }
                else if (purchaseInteraction.TryGetComponent(out RouletteChestController rouletteChestController))
                {
                    rouletteChestController.dropCount++;
                }
                else
                {
                    continue;
                }
                
                var attachment = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("GSvs/RoR2/Items/SaleStar/SaleStarChestAttachment.prefab").WaitForCompletion(), purchaseInteraction.transform.position, Quaternion.identity, purchaseInteraction.transform);
                attachment.GetComponent<SaleStarChestAttachment>().SetPurchaseInteractionServer(purchaseInteraction);
                NetworkServer.Spawn(attachment);

                Debug.Log($"Affected {purchaseInteraction.name}");
                if (--remainingAffectedChestsCount <= 0)
                {
                    break;
                }
            }
        }
    }
}
