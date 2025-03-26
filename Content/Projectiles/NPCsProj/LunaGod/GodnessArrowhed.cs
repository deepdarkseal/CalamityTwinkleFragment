using CalamityMod.Buffs.DamageOverTime;
using Calamitytwinklefragment.Content.Buff.Debuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Calamitytwinklefragment.Content.Projectiles.NPCsProj.LunaGod
{
    class GodnessArrowhed : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.hostile = true;
            Projectile.timeLeft = 300; // 弹幕存在时间
            Projectile.Calamity().DealsDefenseDamage = true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 180);
            target.AddBuff(ModContent.BuffType<Nightwither>(), 120);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;//修改弹幕的最大绘制距离
        }
    }
}

