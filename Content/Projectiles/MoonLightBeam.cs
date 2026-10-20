using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    /// <summary>
    /// 月光波：附魔重击释放的月牙形剑气，可穿透多个敌人。
    /// </summary>
    internal class MoonLightBeam : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.scale = 1.1f;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            Lighting.AddLight(Projectile.Center, 0.4f, 0.6f, 1f);

            // 尾段淡出。
            if (Projectile.timeLeft < 22)
                Projectile.alpha = Math.Min(255, Projectile.alpha + 11);

            if (Main.rand.NextBool(2))
            {
                Vector2 spawn = Projectile.Center + Main.rand.NextVector2Circular(26f, 58f);

                Dust dust = Dust.NewDustPerfect(
                    spawn,
                    DustID.Frost,
                    -Projectile.velocity * 0.045f,
                    100,
                    new Color(145, 190, 255),
                    Main.rand.NextFloat(0.9f, 1.3f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 240);
            target.AddBuff(ModContent.BuffType<Nightwither>(), 240);
            target.AddBuff(BuffID.Chilled, 180);

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.Frost,
                    Main.rand.NextVector2Circular(4f, 4f),
                    100,
                    new Color(150, 195, 255),
                    Main.rand.NextFloat(1f, 1.6f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            float opacity = 1f - Projectile.alpha / 255f;

            Color edgeColor = new Color(95, 145, 255);
            Color coreColor = new Color(200, 232, 255);

            // 拖尾残影。
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float progress = i / (float)Projectile.oldPos.Length;
                float fade = 1f - progress;
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    texture,
                    drawPosition,
                    null,
                    edgeColor * (fade * 0.25f * opacity),
                    Projectile.rotation,
                    origin,
                    Projectile.scale * (0.8f + fade * 0.2f),
                    SpriteEffects.None,
                    0);
            }

            Vector2 center = Projectile.Center - Main.screenPosition;

            // 外层辉光、主体、亮芯。
            Main.EntitySpriteDraw(
                texture,
                center,
                null,
                edgeColor * (0.5f * opacity),
                Projectile.rotation,
                origin,
                Projectile.scale * 1.1f,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                center,
                null,
                coreColor * opacity,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                center,
                null,
                Color.White * (0.45f * opacity),
                Projectile.rotation,
                origin,
                Projectile.scale * 0.72f,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 18; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Frost,
                    Main.rand.NextVector2Circular(5f, 5f),
                    100,
                    new Color(145, 190, 255),
                    Main.rand.NextFloat(1f, 1.6f));
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.35f, Volume = 0.7f }, Projectile.Center);
        }
    }
}
