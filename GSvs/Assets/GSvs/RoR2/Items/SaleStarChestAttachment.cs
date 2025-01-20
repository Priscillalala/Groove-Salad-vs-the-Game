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
            if (balloons)
            {
                balloons.transform.SetParent(null);
                ParticleSystem.Particle[] balloonParticles = new ParticleSystem.Particle[balloons.particleCount];
                balloons.GetParticles(balloonParticles);
                for (int i = 0; i < balloons.particleCount; i++)
                {
                    ref ParticleSystem.Particle particle = ref balloonParticles[i];
                    //particle.remainingLifetime = 10f;
                    particle.position = particle.position;
                    particle.velocity = particle.velocity;
                }
                //var force = balloons.forceOverLifetime;
                //force.enabled = true;
                balloons.SetParticles(balloonParticles);
            }
        }

        /*public void OnDisable()
        {
            if (balloons)
            {
                balloons.transform.SetParent(null);
            }
        }*/

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
    }
}
