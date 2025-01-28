using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using System;
using UnityEngine.AddressableAssets;

namespace GSvs.RoR2.Mechanics
{
    public abstract class Teleporter : ContentManipulator<Teleporter>
    {
        [InjectConfig(desc = "All post-loop teleporters can be aligned to the moon")]
        public static readonly bool Installed = true;

        [InjectConfig(desc = "Teleporters on the Path of the Colossus can never be aligned to the moon")]
        public static readonly bool ExcludeColossusStages = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
            }
        }

        static void OnPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            var iscTeleporterOp = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Teleporters/iscTeleporter.asset");
            if (Run.instance.loopClearCount > 0 && sceneDirector.teleporterSpawnCard == iscTeleporterOp.WaitForCompletion())
            {
                TryOverrideTeleporterSpawnCard(sceneDirector);
            }
            Addressables.Release(iscTeleporterOp);
        }

        public static void TryOverrideTeleporterSpawnCard(SceneDirector sceneDirector)
        {
            if (ExcludeColossusStages
                && sceneDirector.teleporterSpawnCard
                && sceneDirector.teleporterSpawnCard.prefab 
                && sceneDirector.teleporterSpawnCard.prefab.TryGetComponent(out TeleporterInteraction teleporterInteraction)
                )
            {
                PortalSpawner colossusPortalSpawner = Array.Find(teleporterInteraction.portalSpawners, x => x.bannedEventFlag == "FalseSonBossComplete");
                if (colossusPortalSpawner && SceneInfo.instance)
                {
                    SceneDef sceneDef = SceneInfo.instance.sceneDef;
                    if (sceneDef && Array.IndexOf(colossusPortalSpawner.validStages, sceneDef.baseSceneName) >= 0)
                    {
                        return;
                    }
                }
            }
            sceneDirector.teleporterSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("GSvs/RoR2/Mechanics/Teleporter/iscDivineTeleporter.asset").WaitForCompletion();
        }
    }
}
