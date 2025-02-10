using RoR2.EntityLogic;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    public class SetDaggerProjectileLifetime : MonoBehaviour
    {
        public float newLifetime;
        public DelayedEvent deactivateEvent;

        private void Awake()
        {
            deactivateEvent.CallDelayed(newLifetime);
        }
    }
}
