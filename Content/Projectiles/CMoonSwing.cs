using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class CMoonSwing : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 144;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation += 0.5f;
            Projectile.velocity *= 0.95f;

            // 月光特效
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Enchanted_Gold, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.5f;
            }

            // 逐渐淡出
            Projectile.alpha += 8;
            if (Projectile.alpha > 255)
                Projectile.alpha = 255;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            // 绘制拖尾
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                Color color = Color.Lerp(Color.White * 0.3f, Color.Transparent, progress);
                float scale = Projectile.scale * (1f - progress * 0.5f);

                Main.EntitySpriteDraw(texture,
                    Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null,
                    color * (1f - Projectile.alpha / 255f),
                    Projectile.rotation + i * 0.2f,
                    texture.Size() / 2f,
                    scale,
                    SpriteEffects.None,
                    0);
            }

            // 绘制主体
            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * (1f - Projectile.alpha / 255f),
                Projectile.rotation,
                texture.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }
    }
}
