using GSvs.Core.Events;
using HarmonyLib;
using RoR2;
using System;
using UnityEngine.Networking;

namespace GSvs.RoR2
{
    [HarmonyPatch(typeof(ChestBehavior))]
    public class ChestEvents : EventsPatcher<ChestEvents>
    {
        private static Action<ChestBehavior> onChestItemDropServer;

        public static event Action<ChestBehavior> OnChestItemDropServer
        {
            add
            {
                Patch();
                onChestItemDropServer += value;
            }
            remove => onChestItemDropServer -= value;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(ChestBehavior.ItemDrop))]
        private static void ItemDrop(ChestBehavior __instance)
        {
            if (NetworkServer.active)
            {
                onChestItemDropServer(__instance);
            }
        }
    }
}