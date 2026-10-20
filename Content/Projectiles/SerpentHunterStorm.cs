using Calamitytwinklefragment.Content.Items.Weapons.Melee;
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
    /// 附着在狩猎大蛇矛上的剑气，不飞行。
    /// 每帧同步玩家的 itemRotation，沿矛身方向从矛尖向外延伸；判定是一段线段，
    /// 相当于把矛加长，而不是甩出去一道波。
    /// ai[0] = 1 时为右键突刺，延伸得更远。
    /// </summary>
    internal class SerpentHunterStorm : ModProjectile
    {
        /// <summary>剑气起点距玩家中心的距离，大致对应矛身中前段。</summary>
        private const float StartOffset = 58f;

        /// <summary>横扫时的剑气长度（从起点算起）。</summary>
        private const float SwingLength = 182f;

        /// <summary>突刺时的剑气长度，比横扫更长。</summary>
        private const float ThrustLength = 238f;

        private bool Thrust => Projectile.ai[0] >= 1f;

        public override void SetDefaults()
        {
            // 判定由 Colliding 的线段负责，这里的矩形只是弹幕自身的位置参考。
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 24;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // 挥动期间每个目标只结算一次
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 玩家死亡、切换物品时剑气立即消失，避免留下悬空判定。
            if (!player.active || player.dead || player.HeldItem.type != ModContent.ItemType<SerpentHunter>())
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.localAI[0] == 0f)
            {
                // 等待攻击动画真正开始；Shoot 与动画初始化之间可能有一帧空档。
                if (player.itemAnimation > 0)
                    Projectile.localAI[0] = 1f;
            }
            else if (player.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }

            // 剑气始终挂在矛上：位置取玩家中心，角度取原版武器的挥发角度。
            Projectile.Center = player.MountedCenter;
            Projectile.rotation = player.itemRotation;
            Projectile.velocity = Vector2.Zero;

            // 让弹幕活过整个攻击动画；动画结束后由上面的分支收掉。
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, player.itemAnimation);

            Vector2 direction = Projectile.rotation.ToRotationVector2();
            float start = StartOffset;
            float end = Thrust ? ThrustLength : SwingLength;

            Lighting.AddLight(Projectile.Center + direction * (end * 0.6f), 0.35f, 0.55f, 0.9f);

            // 沿矛身每 3 帧落一点电光，表现剑气附着而非独立飞行。
            if (Main.rand.NextBool(3))
            {
                Vector2 spawnPosition = Projectile.Center + direction * Main.rand.NextFloat(start, end);
                Vector2 sideways = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.6f, 1.6f);

                Dust dust = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.Electric,
                    sideways,
                    100,
                    new Color(150, 205, 255),
                    Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 首帧 itemRotation 尚未同步，先不判定，避免误伤。
            if (Projectile.localAI[0] == 0f)
                return false;

            Vector2 direction = Projectile.rotation.ToRotationVector2();
            float start = StartOffset * 0.8f;
            float end = Thrust ? ThrustLength : SwingLength;

            // 给判定线一点宽容度，避免细线从大体型体节的边缘擦过。
            Rectangle widenedTarget = targetHitbox;
            widenedTarget.Inflate(10, 10);

            return Collision.CheckAABBvLineCollision(
                new Vector2(widenedTarget.X, widenedTarget.Y),
                new Vector2(widenedTarget.Width, widenedTarget.Height),
                Projectile.Center + direction * start,
                Projectile.Center + direction * end);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 剑气就是矛的延长段，对巨蛇的增伤与矛本身一致。
            if (SerpentHunter.IsWormBoss(target))
                modifiers.FinalDamage *= SerpentHunter.WormDamageMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.Electric,
                    Main.rand.NextVector2Circular(4f, 4f),
                    100,
                    new Color(160, 220, 255),
                    Main.rand.NextFloat(0.9f, 1.4f));
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.25f, Volume = 0.4f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 direction = Projectile.rotation.ToRotationVector2();

            float start = StartOffset;
            float end = Thrust ? ThrustLength : SwingLength;
            float length = end - start;
            float rotation = Projectile.rotation + MathHelper.PiOver2;
            float opacity = 1f - Projectile.alpha / 255f;

            Color edgeColor = new Color(95, 165, 225);
            Color coreColor = new Color(205, 235, 255);

            // 沿矛身排四段光刃：越远越小、越淡，形成从矛尖伸出的锥形延长段。
            const int segments = 4;
            for (int i = 0; i < segments; i++)
            {
                float progress = i / (float)(segments - 1);
                float distance = start + length * progress;
                float scale = MathHelper.Lerp(1.45f, 0.7f, progress);
                float alpha = MathHelper.Lerp(0.8f, 0.22f, progress);

                Vector2 position = Projectile.Center + direction * distance - Main.screenPosition;

                Main.EntitySpriteDraw(
                    texture,
                    position,
                    null,
                    edgeColor * (alpha * opacity),
                    rotation,
                    origin,
                    scale * 1.12f,
                    SpriteEffects.None,
                    0);

                Main.EntitySpriteDraw(
                    texture,
                    position,
                    null,
                    coreColor * (alpha * 0.65f * opacity),
                    rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0);
            }

            // 矛尖附近的高光，强调剑气是从武器本身长出来的。
            Vector2 tipPosition = Projectile.Center + direction * (start + length * 0.2f) - Main.screenPosition;
            Main.EntitySpriteDraw(
                texture,
                tipPosition,
                null,
                Color.White * (0.45f * opacity),
                rotation,
                origin,
                1f,
                SpriteEffects.None,
                0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 direction = Projectile.rotation.ToRotationVector2();
            float end = Thrust ? ThrustLength : SwingLength;

            // 剑气收束时沿矛身散开，而不是原地爆炸。
            for (int i = 0; i < 10; i++)
            {
                Vector2 spawnPosition = Projectile.Center + direction * Main.rand.NextFloat(StartOffset, end);
                Dust dust = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.Electric,
                    direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.5f, 1.5f),
                    100,
                    new Color(150, 205, 255),
                    Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }
    }
}
