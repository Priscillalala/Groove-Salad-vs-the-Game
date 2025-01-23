using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    [ContentManipulator]
    [Config(section = "Elusive Antlers Adjustment")]
    public class ElusiveAntlers : StaticContentManipulator<ElusiveAntlers>
    {
        [Config("Max Pickup Lifetime", description = "Please note that Elusive Antlers pickups will also despawn after one minute if the player is far enough away")]
        public static float pickupLifetime = 90f;

        [AssetManipulator]
        private static void SetAntlersPickupDuration(
            [LoadAsset("RoR2/DLC2/Items/SpeedBoostPickup/ElusiveAntlersPickup.prefab")] GameObject ElusiveAntlersPickup
            )
        {
            Debug.Log($"SetAntlersPickupDuration:");
            if (!ElusiveAntlersPickup.TryGetComponent(out DestroyOnTimer destroyOnTimer))
            {
                destroyOnTimer = ElusiveAntlersPickup.AddComponent<DestroyOnTimer>();
            }
            destroyOnTimer.duration = pickupLifetime;
            Transform AntlerShieldBuffPickupVFX = ElusiveAntlersPickup.transform.Find("AntlerShieldBuffPickupVFX");
            if (AntlerShieldBuffPickupVFX)
            {
                if (!ElusiveAntlersPickup.TryGetComponent(out BeginRapidlyActivatingAndDeactivating beginRapidlyActivatingAndDeactivating))
                {
                    beginRapidlyActivatingAndDeactivating = ElusiveAntlersPickup.AddComponent<BeginRapidlyActivatingAndDeactivating>();
                }
                beginRapidlyActivatingAndDeactivating.enabled = true;
                beginRapidlyActivatingAndDeactivating.blinkFrequency = 6f;
                beginRapidlyActivatingAndDeactivating.delayBeforeBeginningBlinking = pickupLifetime - 1f;
                beginRapidlyActivatingAndDeactivating.blinkingRootObject = AntlerShieldBuffPickupVFX.gameObject;
            }
        }
    }
}
