using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class TerminalSoundWave : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 350;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 14; // 独立无敌帧
        }

        public override void AI()
        {
            // 音波的扩散效果
            Projectile.scale += 0.7f;
            Projectile.alpha += 7;

            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
        }
    }
}
