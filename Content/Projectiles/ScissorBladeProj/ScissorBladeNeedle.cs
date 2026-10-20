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
    /// 撕裂之针：前四段剪击甩出的穿透针，参考鸿蒙方舟的 RendingNeedle。
    /// </summary>
    internal class ScissorBladeNeedle : ModProjectile
    {
        public override string Texture => "Calamitytwinklefragment/Content/Projectiles/ScissorBladeProj/ScissorBladeNeedle";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 30;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Blood,
                    -Projectile.velocity * 0.05f,
                    100,
                    default,
                    Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.35f }, target.Center);
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
                Projectile.scale * 1.4f,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                new Color(255, 225, 230),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }
    }
}
