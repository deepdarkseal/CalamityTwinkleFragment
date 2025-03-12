using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calamitytwinklefragment.Content.Buff;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories
{
    public class WhipHookBladePlayer : ModPlayer
    {
        public bool CuttingDebuff = false;

        public override void ResetEffects()
        {
            CuttingDebuff = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CuttingDebuff && ProjectileID.Sets.IsAWhip[proj.type]) // 判断是否为鞭子
            {
                target.AddBuff(ModContent.BuffType<Buff.Cutting>(), 300);
                target.AddBuff(BuffID.Bleeding, 300);
            }
        }
    }
}