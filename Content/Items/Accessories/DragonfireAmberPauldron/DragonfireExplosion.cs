using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Calamitytwinklefragment.Content.Items.Accessories.DragonfireAmberPauldron
{
    public class DragonfireExplosion : ModProjectile
    {
        public override string Texture => "Calamitytwinklefragment/Content/Items/Accessories/DragonfireAmberPauldron/DragonfireExplosion"; // 爆炸贴图路径

        public override void SetDefaults()
        {
            Projectile.width = 20;  // 爆炸宽度
            Projectile.height = 20; // 爆炸高度
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20; // 持续时间
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1; // 禁用原版AI
        }

        public override void AI()
        {
            // 爆炸范围逐渐扩大
            Projectile.scale += 0.1f;

            // 火焰粒子
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2),
                    DustID.Torch, // 火焰粒子
                    Main.rand.NextVector2Circular(3f, 3f),
                    100, default, 2f
                );
                dust.noGravity = true;
            }

            // 烟雾粒子
            if (Main.rand.NextBool(2))
            {
                Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2),
                    DustID.Smoke, // 烟雾粒子
                    Main.rand.NextVector2Circular(2f, 2f),
                    100, default, 1.5f
                ).noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 自定义绘制爆炸（可选）
            return base.PreDraw(ref lightColor);
        }
    }
}
