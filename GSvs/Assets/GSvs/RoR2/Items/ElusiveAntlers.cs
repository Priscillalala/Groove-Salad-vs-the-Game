using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    [AssetManipulator]
    public abstract class ElusiveAntlers : NewContentManipulator<ElusiveAntlers>
    {
        [InjectConfig]
        public static readonly bool Installed = true;

        [InjectConfig(desc = "Please note that Elusive Antlers pickups will also despawn after one minute if the player is far enough away")]
        public static readonly float MaxPickupLifetime = 90f;

        [InitDuringStartup]
        private static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
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
            destroyOnTimer.duration = MaxPickupLifetime;
            Transform AntlerShieldBuffPickupVFX = ElusiveAntlersPickup.transform.Find("AntlerShieldBuffPickupVFX");
            if (AntlerShieldBuffPickupVFX)
            {
                if (!ElusiveAntlersPickup.TryGetComponent(out BeginRapidlyActivatingAndDeactivating beginRapidlyActivatingAndDeactivating))
                {
                    beginRapidlyActivatingAndDeactivating = ElusiveAntlersPickup.AddComponent<BeginRapidlyActivatingAndDeactivating>();
                }
                beginRapidlyActivatingAndDeactivating.enabled = true;
                beginRapidlyActivatingAndDeactivating.blinkFrequency = 6f;
                beginRapidlyActivatingAndDeactivating.delayBeforeBeginningBlinking = MaxPickupLifetime - 1f;
                beginRapidlyActivatingAndDeactivating.blinkingRootObject = AntlerShieldBuffPickupVFX.gameObject;
            }
        }
    }
}
