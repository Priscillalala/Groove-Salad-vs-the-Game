using EntityStates;
using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    public class SaleStarChestAttachment : MonoBehaviour
    {
        public ParticleSystem balloons;
        public GameObject openingEfectPrefab;

        private ChestBehavior chestServer;
        private RouletteChestController rouletteChestServer;

        public void SetPurchaseInteractionServer(PurchaseInteraction purchaseInteraction)
        {
            if (purchaseInteraction.TryGetComponent(out chestServer))
            {
                ChestDropEvents.OnChestItemDropServer += OnChestItemDropServer;
            }
            else if (purchaseInteraction.TryGetComponent(out rouletteChestServer))
            {
                rouletteChestServer.onCycleCompletedServer.AddListener(OnChestOpenedServer);
            }
        }

        private void OnChestItemDropServer(ChestBehavior chest)
        {
            if (chest && chest == chestServer)
            {
                OnChestOpenedServer();
            }
        }

        public void OnChestOpenedServer()
        {
            Vector3 origin = transform.position;
            if (chestServer && chestServer.dropTransform)
            {
                origin = chestServer.dropTransform.position;
            }
            else if (rouletteChestServer && rouletteChestServer.ejectionTransform)
            {
                origin = rouletteChestServer.ejectionTransform.position;
            }
            EffectManager.SpawnEffect(openingEfectPrefab, new EffectData
            {
                origin = origin,
                rotation = transform.rotation,
            }, true);

            Destroy(gameObject);
        }

        public void Start()
        {
            if (balloons)
            {
                var trails = balloons.trails;
                trails.enabled = false;
                balloons.Emit(new ParticleSystem.EmitParams
                {
                    position = transform.position + (Vector3.down * 1000f),
                    startLifetime = 100000,
                }, 1);
                trails.enabled = true;
            }
        }

        public void OnDestroy()
        {
            ChestDropEvents.OnChestItemDropServer -= OnChestItemDropServer;
            if (rouletteChestServer)
            {
                rouletteChestServer.onCycleCompletedServer.RemoveListener(OnChestOpenedServer);
            }
        }

        public void OnDisable()
        {
            if (balloons)
            {
                balloons.transform.SetParent(null);

                /*var force = balloons.forceOverLifetime;
                force.enabled = true;
                var limitVelocityOverLifeTime = balloons.limitVelocityOverLifetime;
                limitVelocityOverLifeTime.enabled = true;*/
                var collision = balloons.collision;
                collision.enabled = true;

                /*ParticleSystem.Particle[] balloonParticles = new ParticleSystem.Particle[balloons.particleCount];
                balloons.GetParticles(balloonParticles);
                for (int i = 0; i < balloons.particleCount; i++)
                {
                    ref ParticleSystem.Particle particle = ref balloonParticles[i];
                    particle.remainingLifetime = 30f;
                    Vector3 velocity = (particle.position - transform.position).normalized * 5f;
                    velocity.y *= 0.5f;
                    particle.velocity = velocity;
                }
                balloons.SetParticles(balloonParticles);*/

                if (balloons.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.AddForce(Vector3.up, ForceMode.Impulse);
                }

                if (balloons.TryGetComponent(out ConstantForce constantForce))
                {
                    constantForce.enabled = true;
                }
                /*if (balloons.TryGetComponent(out DestroyOnParticleEnd destroyOnParticleEnd))
                {
                    destroyOnParticleEnd.enabled = true;
                }*/
                if (balloons.TryGetComponent(out DestroyOnTimer destroyOnTimer))
                {
                    destroyOnTimer.enabled = true;
                }
                if (balloons.TryGetComponent(out Collider collider))
                {
                    collider.enabled = true;
                }
            }
        }
    }
}
