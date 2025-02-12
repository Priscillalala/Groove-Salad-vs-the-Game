using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;

namespace GSvs.RoR2.Items
{
    [AssetManipulator]
    public abstract class WarpedEcho : ContentManipulator<WarpedEcho>
    {
        [InjectConfig(desc = "Remove Warped Echo")]
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
        static void RemoveItem([LoadAsset("RoR2/DLC2/Items/DelayedDamage/DelayedDamage.asset")] ItemDef DelayedDamage)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            DelayedDamage.deprecatedTier = ItemTier.NoTier;
#pragma warning restore CS0618 // Type or member is obsolete
            DelayedDamage._itemTierDef = null;
        }
    }
}
