using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSvs.RoR2.Interactables
{
    public class ModifedScrapper : MonoBehaviour
    {
        PickupPickerController pickupPickerController;

        private void Awake()
        {
            if (TryGetComponent(out pickupPickerController))
            {
                var onServerInteractionBegin = pickupPickerController.onServerInteractionBegin;
                for (int i = 0; i < onServerInteractionBegin.GetPersistentEventCount(); i++)
                {
                    if (onServerInteractionBegin.GetPersistentMethodName(i) == nameof(PickupPickerController.SetOptionsFromInteractor))
                    {
                        onServerInteractionBegin.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                        break;
                    }
                }
                onServerInteractionBegin.AddListener(SetOptionsForScrapper);
            }
        }

        private void SetOptionsForScrapper(Interactor interactor)
        {
            GSvsPlugin.Logger.LogMessage("Set scrapping options");
            if (!interactor || !interactor.TryGetComponent(out CharacterBody interactorBody))
            {
                return;
            }
            Inventory inventory = interactorBody.inventory;
            if (!inventory)
            {
                return;
            }
            if (!inventory.TryGetComponent(out InventoryScrapperOptions scrapperOptions))
            {
                scrapperOptions = inventory.gameObject.AddComponent<InventoryScrapperOptions>();
            }
            Xoroshiro128Plus selectionRng = scrapperOptions.GetSelectionRng();
            var itemDefsByTier = ItemCatalog.allItemDefs.GroupBy(x => x.tier);
            List<ItemDef> allScrappableItemDefs = new List<ItemDef>();
            foreach (var group in itemDefsByTier)
            {
                ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(group.Key);
                if (!itemTierDef || itemTierDef.canScrap)
                {
                    allScrappableItemDefs.AddRange(group.Where(x => x.canRemove && !x.hidden && x.DoesNotContainTag(ItemTag.Scrap)));
                }
            }
            Util.ShuffleList(allScrappableItemDefs, selectionRng);
            int optionsCount = Mathf.Min(3, inventory.itemAcquisitionOrder.Count);
            var options = allScrappableItemDefs
                .Where(x => inventory.GetItemCount(x) > 0)
                .Take(optionsCount)
                .Select(x => new PickupPickerController.Option
                {
                    pickupIndex = PickupCatalog.FindPickupIndex(x.itemIndex),
                    available = true,
                });
            pickupPickerController.SetOptionsServer(options.ToArray());
        }
    }
}
