using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace GSvs.RoR2.Mechanics
{
    // TODO: custom teleporter prefab
    public abstract class Teleporter : ContentManipulator<Teleporter>
    {
        [InjectConfig(desc = "All post-loop teleporters can be aligned to the moon")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
            }
        }

        private static void OnPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            var iscTeleporterOp = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Teleporters/iscTeleporter.asset");
            if (Run.instance.loopClearCount >= 0 && sceneDirector.teleporterSpawnCard == iscTeleporterOp.WaitForCompletion())
            {
                sceneDirector.teleporterSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("GSvs/RoR2/Mechanics/Teleporter/iscDivineTeleporter.asset").WaitForCompletion();
            }
            Addressables.Release(iscTeleporterOp);
        }
    }
}
