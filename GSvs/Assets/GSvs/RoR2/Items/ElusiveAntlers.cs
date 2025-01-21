using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GSvs.RoR2.Items
{
    [ContentModification]
    public class ElusiveAntlers : IContentModification
    {
        public void Initialize()
        {
            Debug.Log("ElusiveAntlers init");
        }

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
            destroyOnTimer.duration = 90f;
            Transform AntlerShieldBuffPickupVFX = ElusiveAntlersPickup.transform.Find("AntlerShieldBuffPickupVFX");
            if (AntlerShieldBuffPickupVFX)
            {
                if (!ElusiveAntlersPickup.TryGetComponent(out BeginRapidlyActivatingAndDeactivating beginRapidlyActivatingAndDeactivating))
                {
                    beginRapidlyActivatingAndDeactivating = ElusiveAntlersPickup.AddComponent<BeginRapidlyActivatingAndDeactivating>();
                }
                beginRapidlyActivatingAndDeactivating.enabled = true;
                beginRapidlyActivatingAndDeactivating.blinkFrequency = 6f;
                beginRapidlyActivatingAndDeactivating.delayBeforeBeginningBlinking = 89f;
                beginRapidlyActivatingAndDeactivating.blinkingRootObject = AntlerShieldBuffPickupVFX.gameObject;
            }
        }
    }
}
