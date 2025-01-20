using RoR2;
using System;
using UnityEngine.Networking;

namespace GSvs.RoR2
{
    public static class ChestDropEvents
    {
        private static bool initialized;
        private static Action<ChestBehavior> onChestItemDropServer;

        public static event Action<ChestBehavior> OnChestItemDropServer
        {
            add
            {
                if (!initialized)
                {
                    Initialize();
                }
                onChestItemDropServer += value;
            }
            remove
            {
                onChestItemDropServer -= value;
            }
        }

        private static void Initialize()
        {
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
            initialized = true;
        }

        private static void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                onChestItemDropServer(self);
            }
        }
    }
}