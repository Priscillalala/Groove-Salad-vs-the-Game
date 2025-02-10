using GSvs.Core;
using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    [AssetManipulator]
    [LanguageOverrides]
    [HarmonyPatch]
    public abstract class ElusiveAntlers : ContentManipulator<ElusiveAntlers>
    {
        [InjectConfig(desc = "Remove stacking movement speed from Elusive Antlers and limit the lifetime of pickups to reduce lag")]
        public static readonly bool Installed = true;

        [InjectConfig(desc = "Please note that Elusive Antlers pickups will also despawn after one minute if the player is far enough away")]
        public static readonly float MaxPickupLifetime = 90f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetPickupDuration([LoadAsset("RoR2/DLC2/Items/SpeedBoostPickup/ElusiveAntlersPickup.prefab")] GameObject ElusiveAntlersPickup)
        {
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

        [HarmonyILManipulator, HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.RecalculateStats))]
        static void RemoveMovementSpeedIncreaseStacking(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locItemCount = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.SpeedBoostPickup)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out locItemCount)
                );
            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(locItemCount),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul()
                );
            c.Index++;
            c.EmitDelegate<Func<int, int>>(itemCount => Mathf.Min(itemCount, 1));
        }
    }
}
