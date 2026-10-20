using System;
using Calamitytwinklefragment.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    /// <summary>
    /// 举剑附魔动画：剑身竖直举过头顶，月光从四周汇聚。
    /// </summary>
    internal class MoonlightGreatswordHeld : ModProjectile
    {
        private const int Lifetime = 55;
        private static readonly Vector2 HiltOrigin = new(18f, 164f);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.HeldItem.type != ModContent.ItemType<MoonlightGreatsword>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.MountedCenter + new Vector2(0f, -10f);
            Projectile.velocity = Vector2.Zero;

            float progress = 1f - Projectile.timeLeft / (float)Lifetime;
            Projectile.rotation = -MathHelper.PiOver4 + MathHelper.ToRadians(4f) * (float)Math.Sin(progress * MathHelper.TwoPi);

            Vector2 bladeDirection = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
            Vector2 swordTip = Projectile.Center + bladeDirection * 150f;
            Lighting.AddLight(swordTip, 0.5f, 0.7f, 1f);

            // 月光从四周汇聚到剑身。
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(60f, 120f);
                Vector2 start = Projectile.Center + offset;
                Vector2 velocity = (swordTip - start).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2.5f, 4.5f);

                Dust dust = Dust.NewDustPerfect(
                    start,
                    DustID.Frost,
                    velocity,
                    100,
                    new Color(150, 195, 255),
                    Main.rand.NextFloat(1f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.2f;
            }

            // 起手时脚下的月光环。
            if (Projectile.timeLeft == Lifetime - 1)
            {
                for (int i = 0; i < 32; i++)
                {
                    float angle = MathHelper.TwoPi * i / 32f;
                    Vector2 offset = angle.ToRotationVector2() * 72f;

                    Dust dust = Dust.NewDustPerfect(
                        player.MountedCenter + offset,
                        DustID.Frost,
                        offset * 0.02f,
                        100,
                        new Color(150, 195, 255),
                        1.3f);
                    dust.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float fade = Projectile.timeLeft < 12 ? Projectile.timeLeft / 12f : 1f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Color glowColor = new Color(95, 155, 255) * (0.22f * fade);
            Color mainColor = new Color(195, 228, 255) * fade;

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                glowColor,
                Projectile.rotation,
                HiltOrigin,
                0.72f,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                mainColor,
                Projectile.rotation,
                HiltOrigin,
                0.62f,
                SpriteEffects.None,
                0);

            return false;
        }
    }
}
