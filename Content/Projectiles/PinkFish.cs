using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class PinkFish : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 256; // 爆炸范围宽度
            Projectile.height = 256; // 爆炸范围高度
            Projectile.friendly = true; // 对玩家友好
            Projectile.hostile = false; // 不对玩家敌对
            Projectile.penetrate = -1; // 穿透
            Projectile.timeLeft = 120; // 弹幕存在时间
            Projectile.tileCollide = false; // 不与方块碰撞
            Projectile.ignoreWater = true; // 忽略水
        }
    }
}
