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
                ChestEvents.OnChestItemDropServer += OnChestItemDropServer;
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
            if (DelusionChestController.isDelusionEnable && chestServer && chestServer.TryGetComponent(out DelusionChestController delusionChestController))
            {
                if (delusionChestController.isDelusionSelected)
                {
                    if (!delusionChestController.IsSelectedCorrectly())
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                    PlayOpeningEffectServer();
                    return;
                }
            }

            PlayOpeningEffectServer();
            Destroy(gameObject);
        }

        public void PlayOpeningEffectServer()
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
            ChestEvents.OnChestItemDropServer -= OnChestItemDropServer;
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

                var collision = balloons.collision;
                collision.enabled = true;

                if (balloons.TryGetComponent(out ConstantForce constantForce))
                {
                    constantForce.enabled = true;
                }
                if (balloons.TryGetComponent(out DestroyOnTimer destroyOnTimer))
                {
                    destroyOnTimer.enabled = true;
                }
                if (balloons.TryGetComponent(out Collider collider))
                {
                    collider.enabled = true;
                }

                if (balloons.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.AddForce(Vector3.up, ForceMode.Impulse);
                }
            }
        }
    }
}
