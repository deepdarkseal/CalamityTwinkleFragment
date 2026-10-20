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
    /// 大撕裂：满充能时右键在光标处撕开的时空裂缝。
    /// ai[0]：裂缝朝向；持续 40 帧，对触碰到的敌人各结算一次。
    /// </summary>
    internal class ScissorBladeRift : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.ai[0];
            Projectile.velocity = Vector2.Zero;

            // 裂缝边缘不断渗出红黑色的碎屑。
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(-140f, 140f);
                Vector2 spawnPosition = Projectile.Center + offset
                    + Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-16f, 16f);

                Dust dust = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.Blood,
                    Main.rand.NextVector2Circular(2f, 2f),
                    100,
                    default,
                    Main.rand.NextFloat(0.9f, 1.4f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.1f, 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.45f, Volume = 0.8f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            float scaleX = Projectile.width / (float)texture.Width;
            float scaleY = Projectile.height / (float)texture.Height;

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                new Color(120, 10, 20) * 0.65f,
                Projectile.rotation,
                origin,
                new Vector2(scaleX * 1.35f, scaleY * 1.6f),
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                new Color(255, 210, 215),
                Projectile.rotation,
                origin,
                new Vector2(scaleX, scaleY),
                SpriteEffects.None,
                0);

            return false;
        }
    }
}
