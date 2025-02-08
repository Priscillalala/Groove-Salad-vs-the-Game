using RoR2;
using System.Collections.Generic;
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
            if (!inventory || !inventory.TryGetComponent(out ScrapperInventory scrapperInventory))
            {
                return;
            }
            List<PickupPickerController.Option> options = new List<PickupPickerController.Option>();
            foreach (ItemIndex itemIndex in inventory.itemAcquisitionOrder)
            {
                if (!scrapperInventory.collectedItemCounts.ContainsKey(itemIndex))
                {
                    continue;
                }
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemDef.tier);
                if ((!itemTierDef || itemTierDef.canScrap) && itemDef.canRemove && !itemDef.hidden && itemDef.DoesNotContainTag(ItemTag.Scrap))
                {
                    options.Add(new PickupPickerController.Option
                    {
                        pickupIndex = PickupCatalog.FindPickupIndex(itemIndex),
                        available = true,
                    });
                }
            }
            pickupPickerController.SetOptionsServer(options.ToArray());
        }
    }
}
