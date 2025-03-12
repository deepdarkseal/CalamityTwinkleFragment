using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Buffs.DamageOverTime;
using Calamitytwinklefragment.Content.Buff.Debuffs;
using Terraria.ID;
using Microsoft.Xna.Framework;
using CalamityMod;

namespace Calamitytwinklefragment.Content.Projectiles.NPCsProj.LunaGod
{
    class LunaSilkProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.timeLeft = 1800; // 弹幕存在时间
            Projectile.Calamity().DealsDefenseDamage = true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 180);
            target.AddBuff(ModContent.BuffType<Nightwither>(), 120);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 自定义绘制逻辑
            return true;
        }
    }
}
