using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace GSvs.RoR2.Monsters
{
    [AssetManipulator]
    public abstract class GreaterWisp : ContentManipulator<GreaterWisp>
    {
        [InjectConfig(desc = "Reduce the cost of Greater Wisps and add fire trails to their cannons")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly int Cost = 100;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetCost([LoadAsset("RoR2/Base/GreaterWisp/cscGreaterWisp.asset")] CharacterSpawnCard cscGreaterWisp)
        {
            cscGreaterWisp.directorCreditCost = Cost;
        }

        [AssetManipulator]
        static void AddFireTrails(
            [LoadAsset("RoR2/Base/GreaterWisp/WispCannon.prefab")] GameObject WispCannon,
            [LoadAsset("GSvs/RoR2/Monsters/GreaterWisp/WispGroundCannon.prefab")] GameObject WispGroundCannon
            )
        {
            if (WispCannon.TryGetComponent(out ProjectileImpactExplosion projectileImpactExplosion))
            {
                projectileImpactExplosion.fireChildren = true;
                projectileImpactExplosion.childrenProjectilePrefab = WispGroundCannon;
                projectileImpactExplosion.childrenCount = 1;
                projectileImpactExplosion.childrenDamageCoefficient = 1;
                projectileImpactExplosion.transformSpace = ProjectileImpactExplosion.TransformSpace.Local;
                projectileImpactExplosion.minAngleOffset = new Vector3(0f, 0f, 1f);
                projectileImpactExplosion.maxAngleOffset = new Vector3(0f, 0f, 1f);
            }
        }
    }
}
