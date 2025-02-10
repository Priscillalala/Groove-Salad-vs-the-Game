using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using RoR2;
using RoR2.EntityLogic;
using UnityEngine;
using UnityEngine.Events;

namespace GSvs.RoR2.Items
{
    [AssetManipulator]
    public abstract class CeremonialDagger : ContentManipulator<CeremonialDagger>
    {
        [InjectConfig(desc = "Reduce the lifetime of Ceremonial Dagger projectiles")]
        public static readonly bool Installed = true;

        [InjectConfig]
        public static readonly float ProjectileLifetime = 4f;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [AssetManipulator]
        static void SetProjectileLifetime([LoadAsset("RoR2/Base/Dagger/DaggerProjectile.prefab")] GameObject DaggerProjectile)
        {
            Transform deactivateDagger = DaggerProjectile.transform.Find("DeactivateDagger");
            if (deactivateDagger && deactivateDagger.TryGetComponent(out DelayedEvent deactivateEvent))
            {
                if (DaggerProjectile.TryGetComponent(out AwakeEvent awakeEvent))
                {
                    for (int i = 0; i < awakeEvent.action.GetPersistentEventCount(); i++)
                    {
                        if (awakeEvent.action.GetPersistentTarget(i) == deactivateEvent)
                        {
                            GSvsPlugin.Logger.LogWarning("Removed Daggers event!");
                            awakeEvent.action.SetPersistentListenerState(i, UnityEventCallState.Off);
                            break;
                        }
                    }
                }
                SetDaggerProjectileLifetime setDaggerProjectileLifetime = DaggerProjectile.AddComponent<SetDaggerProjectileLifetime>();
                setDaggerProjectileLifetime.newLifetime = ProjectileLifetime;
                setDaggerProjectileLifetime.deactivateEvent = deactivateEvent;
            }
            if (DaggerProjectile.TryGetComponent(out DestroyOnTimer destroyOnTimer))
            {
                destroyOnTimer.duration = ProjectileLifetime + 3f;
            }
        }
    }
}
