using GSvs.Core.Util;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace GSvs.RoR2.Items
{
    public class DelicateWatchBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => DelicateWatch.Installed ? DLC1Content.Items.FragileDamageBonus : null;

        public static BuffDef Buff => GSvsRoR2Content.Buffs.DelicateWatchBonus;
        public int MaxBuffStacks => StackUtil.Scale(DelicateWatch.MaxBuffs, DelicateWatch.MaxBuffsPerStack, stack);

        private float grantBuffTimer;

        private void FixedUpdate()
        {
            if (body.outOfDanger)
            {
                if (body.GetBuffCount(Buff) < MaxBuffStacks)
                {
                    grantBuffTimer -= Time.fixedDeltaTime;
                    if (grantBuffTimer <= 0)
                    {
                        grantBuffTimer += 1f;
                        body.AddBuff(Buff);
                    }
                }
            }
            else
            {
                body.SetBuffCount(Buff.buffIndex, 0);
                grantBuffTimer = 0f;
            }
        }

        private void OnDisable()
        {
            body.SetBuffCount(Buff.buffIndex, 0);
        }
    }
}
