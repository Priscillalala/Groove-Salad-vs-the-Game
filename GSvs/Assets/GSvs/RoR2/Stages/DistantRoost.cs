using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using System;
using UnityEngine;

namespace GSvs.RoR2.Stages
{
    [AssetManipulator]
    public abstract class DistantRoost : ContentManipulator<DistantRoost>
    {
        [InjectConfig(desc = "Allow Jellyfish to spawn on Distant Roost pre-loop")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void ModifyMonstersSelection(
            [LoadAsset("RoR2/Base/blackbeach/dccsBlackBeachMonsters.asset")] DirectorCardCategorySelection dccsBlackBeachMonsters,
            [LoadAsset("RoR2/Base/blackbeach/dccsBlackBeachMonstersDLC.asset")] DirectorCardCategorySelection dccsBlackBeachMonstersDLC,
            [LoadAsset("RoR2/DLC2/dccsBlackBeachMonstersDLC2.asset")] DirectorCardCategorySelection dccsBlackBeachMonstersDLC2,
            [LoadAsset("RoR2/DLC2/dccsBlackBeachMonstersDLC2Only.asset")] DirectorCardCategorySelection dccsBlackBeachMonstersDLC2Only,
            [LoadAsset("RoR2/Base/Jellyfish/cscJellyfish.asset")] CharacterSpawnCard cscJellyfish
            )
        {
            ModifyDccs(dccsBlackBeachMonsters);
            ModifyDccs(dccsBlackBeachMonstersDLC);
            ModifyDccs(dccsBlackBeachMonstersDLC2);
            ModifyDccs(dccsBlackBeachMonstersDLC2Only);

            void ModifyDccs(DirectorCardCategorySelection dccs)
            {
                ref var basicMonsters = ref dccs.categories[dccs.FindCategoryIndexByName("Basic Monsters")];
                DirectorCard jellyfishCard = Array.Find(basicMonsters.cards, x => x.spawnCard == cscJellyfish);
                if (jellyfishCard != null)
                {
                    jellyfishCard.minimumStageCompletions = 0;
                }
            }
        }
    }
}
