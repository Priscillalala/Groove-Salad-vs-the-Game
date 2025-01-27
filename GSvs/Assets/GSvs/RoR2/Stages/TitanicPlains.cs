using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using System;
using UnityEngine;

namespace GSvs.RoR2.Stages
{
    [AssetManipulator]
    public abstract class TitanicPlains : ContentManipulator<TitanicPlains>
    {
        [InjectConfig(desc = "Balance Stone Golem and Lemurian selection weights on Titanic Plains")]
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
            [LoadAsset("RoR2/Base/golemplains/dccsGolemplainsMonsters.asset")] DirectorCardCategorySelection dccsGolemplainsMonsters,
            [LoadAsset("RoR2/Base/golemplains/dccsGolemplainsMonstersDLC1.asset")] DirectorCardCategorySelection dccsGolemplainsMonstersDLC1,
            [LoadAsset("RoR2/DLC2/dccsGolemplainsMonstersDLC2.asset")] DirectorCardCategorySelection dccsGolemplainsMonstersDLC2,
            [LoadAsset("RoR2/DLC2/dccsGolemplainsMonstersDLC2Only.asset")] DirectorCardCategorySelection dccsGolemplainsMonstersDLC2Only,
            [LoadAsset("RoR2/Base/Golem/cscGolem.asset")] CharacterSpawnCard cscGolem,
            [LoadAsset("RoR2/Base/Lemurian/cscLemurian.asset")] CharacterSpawnCard cscLemurian
            )
        {
            ModifyDccs(dccsGolemplainsMonsters);
            ModifyDccs(dccsGolemplainsMonstersDLC1);
            ModifyDccs(dccsGolemplainsMonstersDLC2);
            ModifyDccs(dccsGolemplainsMonstersDLC2Only);

            void ModifyDccs(DirectorCardCategorySelection dccs)
            {
                ref var minibosses = ref dccs.categories[dccs.FindCategoryIndexByName("Minibosses")];
                minibosses.selectionWeight = 3;
                DirectorCard golemCard = Array.Find(minibosses.cards, x => x.spawnCard == cscGolem);
                if (golemCard != null)
                {
                    golemCard.selectionWeight = 2;
                }

                ref var basicMonsters = ref dccs.categories[dccs.FindCategoryIndexByName("Basic Monsters")];
                DirectorCard lemurianCard = Array.Find(basicMonsters.cards, x => x.spawnCard == cscLemurian);
                if (golemCard != null)
                {
                    golemCard.selectionWeight = 3;
                }
            }
        }
    }
}
