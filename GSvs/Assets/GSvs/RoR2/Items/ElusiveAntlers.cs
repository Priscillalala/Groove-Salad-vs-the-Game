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
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/SpeedBoostPickup/ElusiveAntlersPickup.prefab").Completed += op =>
            {
                GameObject ElusiveAntlersPickup = op.Result;
                if (!ElusiveAntlersPickup.TryGetComponent(out DestroyOnTimer destroyOnTimer))
                {
                    destroyOnTimer = ElusiveAntlersPickup.AddComponent<DestroyOnTimer>();
                }
                destroyOnTimer.duration = 120f;
                Transform AntlerShieldBuffPickupVFX = ElusiveAntlersPickup.transform.Find("AntlerShieldBuffPickupVFX");
                if (AntlerShieldBuffPickupVFX)
                {
                    if (!ElusiveAntlersPickup.TryGetComponent(out BeginRapidlyActivatingAndDeactivating beginRapidlyActivatingAndDeactivating))
                    {
                        beginRapidlyActivatingAndDeactivating = ElusiveAntlersPickup.AddComponent<BeginRapidlyActivatingAndDeactivating>();
                    }
                    beginRapidlyActivatingAndDeactivating.enabled = true;
                    beginRapidlyActivatingAndDeactivating.blinkFrequency = 4f;
                    beginRapidlyActivatingAndDeactivating.delayBeforeBeginningBlinking = 118f;
                    beginRapidlyActivatingAndDeactivating.blinkingRootObject = AntlerShieldBuffPickupVFX.gameObject;
                }
            };
        }
    }
}
