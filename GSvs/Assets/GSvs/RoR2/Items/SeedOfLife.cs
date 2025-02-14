using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;

namespace GSvs.RoR2.Items
{
    // TODO: More comprehensive changes?
    [AssetManipulator]
    public abstract class SeedOfLife : ContentManipulator<SeedOfLife>
    {
        [InjectConfig(desc = "Seed of Life only appears in multiplayer")]
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
        static void ModifyEquipment([LoadAsset("RoR2/DLC2/HealAndRevive/HealAndRevive.asset")] EquipmentDef HealAndRevive)
        {
            HealAndRevive.appearsInSinglePlayer = false;
        }
    }
}
