using GSvs.Core.Events;
using HarmonyLib;
using RoR2;
using System;
using UnityEngine.Networking;

namespace GSvs.Events
{
    [HarmonyPatch(typeof(ChestBehavior))]
    public abstract class ChestEvents : EventsPatcher<ChestEvents>
    {
        private static Action<ChestBehavior> onChestItemDropServer;

        public static event Action<ChestBehavior> OnChestItemDropServer
        {
            add
            {
                PatchAll();
                onChestItemDropServer += value;
            }
            remove => onChestItemDropServer -= value;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(ChestBehavior.ItemDrop))]
        private static void ItemDrop(ChestBehavior __instance)
        {
            if (NetworkServer.active)
            {
                onChestItemDropServer?.Invoke(__instance);
            }
        }
    }
}