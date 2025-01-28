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
    public abstract class PostLoopBossMusic : ContentManipulator<PostLoopBossMusic>
    {
        [InjectConfig(desc = "Replaces the boss (teleporter) music of Distant Roost, Titanic Plains, and Siphoned Forest post-loop")]
        public static readonly bool Installed = true;

        private static MusicTrackDef overrideBossTrack;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
                SceneCatalog.onMostRecentSceneDefChanged += OnMostRecentSceneDefChanged;
            }
        }

        static void OnMostRecentSceneDefChanged(SceneDef sceneDef)
        {
            MusicTrackDef newOverrideBossTrack = null;
            if (Run.instance && Run.instance.loopClearCount > 0)
            {
                newOverrideBossTrack = sceneDef.baseSceneName switch
                {
                    // Hydrophobia
                    "blackbeach" or "golemplains" => Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muSong16.asset").WaitForCompletion(),
                    // A Boat Made from a Sheet of Newspaper
                    "snowyforest" => Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/DLC1/Common/muBossfightDLC1_12.asset").WaitForCompletion(),
                    _ => null,
                };
            }
            OverrideCurrentBossTrack(newOverrideBossTrack);
        }

        public static void OverrideCurrentBossTrack(MusicTrackDef newOverrideBossTrack)
        {
            MusicController.pickTrackHook -= PickMusicTrack;
            overrideBossTrack = newOverrideBossTrack;
            if (overrideBossTrack != null)
            {
                MusicController.pickTrackHook += PickMusicTrack;
            }
        }

        static void PickMusicTrack(MusicController musicController, ref MusicTrackDef newTrack)
        {
            SceneDef mostRecentSceneDef = SceneCatalog.mostRecentSceneDef;
            if (mostRecentSceneDef && newTrack && newTrack == mostRecentSceneDef.bossTrack)
            {
                newTrack = overrideBossTrack;
            }
        }
    }
}
