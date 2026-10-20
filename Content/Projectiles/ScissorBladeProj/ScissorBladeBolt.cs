using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles.ScissorBladeProj
{
    /// <summary>
    /// 断斩之剪第五段甩出的追踪雷：缓慢转向最近的敌人，穿透 3 次。
    /// </summary>
    internal class ScissorBladeBolt : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 70;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            NPC target = Projectile.Center.ClosestNPCAt(1400f, true);

            if (target != null)
            {
                Vector2 desiredVelocity = (target.Center - Projectile.Center)
                    .SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX * Projectile.direction)) * 20f;

                // 慢慢转向，保留一点“雷霆”而不是精确制导的笨重感。
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.07f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Blood,
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    100,
                    default,
                    Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.3f, Volume = 0.4f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                new Color(220, 35, 55) * 0.6f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.35f,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }
    }
}
