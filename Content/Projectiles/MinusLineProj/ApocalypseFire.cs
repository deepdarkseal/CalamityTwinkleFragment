using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace Calamitytwinklefragment.Content.Projectiles.MinusLineProj
{
    class ApocalypseFire : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1; // 自定义 AI
            Projectile.penetrate = -1;
            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 10; // 独立无敌帧
        }

        public override void AI()
        {
            // 生成烟雾
            if (Main.rand.NextBool(1)) // 控制烟雾生成频率
            {
                Particle smokeGlow = new HeavySmokeParticle(
                    position: Projectile.Center,
                    velocity: Projectile.velocity * 0.2f,
                    color: Color.Violet,
                    lifetime: 30,
                    scale: Main.rand.NextFloat(0.3f, 0.6f),
                    opacity: 0.6f,
                    0,
                    true,
                    0.01f,
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smokeGlow);
            }
        }
    }
}
