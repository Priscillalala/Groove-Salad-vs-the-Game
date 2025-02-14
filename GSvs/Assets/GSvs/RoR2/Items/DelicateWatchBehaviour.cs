using GSvs.Core.Util;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GSvs.RoR2.Items
{
    public class DelicateWatchBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => DelicateWatch.Installed ? DLC1Content.Items.FragileDamageBonus : null;

        public static BuffDef Buff => GSvsRoR2Content.Buffs.DelicateWatchBonus;
        public int MaxBuffCount => StackUtil.Scale(DelicateWatch.MaxBuffs, DelicateWatch.MaxBuffsPerStack, stack);
        public int CurrentBuffCount => body.GetBuffCount(Buff);

        private float grantBuffTimer;
        private bool wasOutOfDanger;
        private GameObject breakEffect;

        private void Start()
        {
            breakEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FragileDamageBonus/DelicateWatchProcEffect.prefab").WaitForCompletion();
        }

        private void FixedUpdate()
        {
            if (body.outOfDanger)
            {
                if (CurrentBuffCount < MaxBuffCount)
                {
                    grantBuffTimer -= Time.fixedDeltaTime;
                    if (grantBuffTimer <= 0)
                    {
                        grantBuffTimer += 1f;
                        body.AddBuff(Buff);
                    }
                }
                wasOutOfDanger = true;
            }
            else
            {
                grantBuffTimer = 0f;
                if (wasOutOfDanger)
                {
                    wasOutOfDanger = false;
                    if (CurrentBuffCount >= MaxBuffCount)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = transform.position,
                        };
                        effectData.SetNetworkedObjectReference(gameObject);
                        EffectManager.SpawnEffect(breakEffect, effectData, true);
                    }
                    body.SetBuffCount(Buff.buffIndex, 0);
                }
            }
        }

        private void OnDisable()
        {
            body.SetBuffCount(Buff.buffIndex, 0);
        }
    }
}
