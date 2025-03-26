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
    public class AbyssalCrushWave : ModProjectile
    {
        public override string Texture => "Calamitytwinklefragment/Content/Items/Accessories/DragonfireAmberPauldron/AbyssalCrushWave"; // 海浪贴图路径

        public override void SetDefaults()
        {
            Projectile.width = 8;  // 海浪宽度
            Projectile.height = 24;  // 海浪高度
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60; // 持续时间
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1; // 禁用原版AI
        }

        public override void AI()
        {
            // 波浪起伏效果
            Projectile.velocity.Y = (float)Math.Sin(Projectile.timeLeft * 0.2f) * 2f; // 上下波动

            // 水花飞溅粒子
            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(
                    Projectile.Center + new Vector2(Main.rand.Next(-Projectile.width / 2, Projectile.width / 2), Projectile.height / 2),
                    DustID.Water, // 水花粒子
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f)),
                    100, default, 1.5f
                ).noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 自定义绘制海浪（可选）
            return base.PreDraw(ref lightColor);
        }
    }
}