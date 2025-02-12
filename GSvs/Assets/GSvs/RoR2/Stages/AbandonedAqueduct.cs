using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HG;
using RoR2;
using System.Linq;

namespace GSvs.RoR2.Stages
{
    [AssetManipulator]
    public abstract class AbandonedAqueduct : ContentManipulator<AbandonedAqueduct>
    {
        [InjectConfig(desc = "Add Children to Abandoned Aqueduct")]
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
            [LoadAsset("RoR2/DLC2/dccsGooLakeMonstersDLC2.asset")] DirectorCardCategorySelection dccsGooLakeMonstersDLC2,
            [LoadAsset("RoR2/DLC2/dccsGooLakeMonstersDLC2Only.asset")] DirectorCardCategorySelection dccsGooLakeMonstersDLC2Only,
            [LoadAsset("RoR2/DLC2/Child/cscChild.asset")] CharacterSpawnCard cscChild
            )
        {
            ModifyDccs(dccsGooLakeMonstersDLC2);
            ModifyDccs(dccsGooLakeMonstersDLC2Only);

            void ModifyDccs(DirectorCardCategorySelection dccs)
            {
                ref var basicMonsters = ref dccs.categories[dccs.FindCategoryIndexByName("Basic Monsters")];
                if (basicMonsters.cards.All(x => x.selectionWeight <= 1))
                {
                    foreach (var card in basicMonsters.cards)
                    {
                        card.selectionWeight *= 2;
                    }
                }
                ArrayUtils.ArrayAppend(ref basicMonsters.cards, new DirectorCard
                {
                    spawnCard = cscChild,
                    selectionWeight = 1,
                });
            }
        }
    }
}
