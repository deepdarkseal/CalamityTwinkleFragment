using System;
using CalamityMod.Buffs.DamageOverTime;
using Calamitytwinklefragment.Content.Items.Weapons.Melee;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    /// <summary>
    /// 月光大剑的挥砍动画与剑刃判定。伤害判定使用随剑身旋转的线段，而不是物品默认的矩形判定框。
    /// ai[0]：0/1/2 为三段连斩，3 为附魔重击；ai[1]：瞄准角度；ai[2]：朝向。
    /// </summary>
    internal class MoonlightGreatswordSwing : ModProjectile
    {
        private static readonly Vector2 HiltOrigin = new(18f, 164f);

        public override void SetDefaults()
        {
            // 默认判定框覆盖整把剑的挥砍范围，真正的命中由 Colliding 的剑刃线段决定。
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 24;
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

            if (Projectile.localAI[0] <= 0f)
                Projectile.localAI[0] = Projectile.timeLeft;

            Projectile.Center = player.MountedCenter;
            Projectile.velocity = Vector2.Zero;

            int mode = (int)Projectile.ai[0];
            float aim = Projectile.ai[1];
            float direction = Projectile.ai[2] < 0f ? -1f : 1f;
            float progress = MathHelper.Clamp(1f - Projectile.timeLeft / Projectile.localAI[0], 0f, 1f);

            GetSwingAngles(mode, aim, direction, out float startAngle, out float endAngle);
            float bladeAngle = MathHelper.Lerp(startAngle, endAngle, Ease(mode, progress));
            Projectile.rotation = bladeAngle + MathHelper.PiOver4;

            bool buffed = player.GetModPlayer<MoonlightGreatswordPlayer>().MoonlightActive;
            Vector2 bladeDirection = bladeAngle.ToRotationVector2();
            Vector2 tip = Projectile.Center + bladeDirection * 142f;

            Lighting.AddLight(
                tip,
                buffed ? 0.5f : 0.22f,
                buffed ? 0.7f : 0.32f,
                buffed ? 1f : 0.55f);

            int dustCount = mode == 3 ? 3 : 1;
            for (int i = 0; i < dustCount; i++)
            {
                if (!Main.rand.NextBool(2))
                    continue;

                Vector2 position = Projectile.Center + bladeDirection * Main.rand.NextFloat(50f, 142f);
                Vector2 tangent = bladeDirection.RotatedBy(MathHelper.PiOver2) * -direction * Main.rand.NextFloat(0.6f, 2.2f);

                Dust dust = Dust.NewDustPerfect(
                    position,
                    DustID.Frost,
                    tangent,
                    100,
                    buffed ? new Color(150, 195, 255) : new Color(170, 190, 220),
                    Main.rand.NextFloat(0.9f, 1.35f));
                dust.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // AI 尚未设置旋转时不做判定，避免第一帧误伤。
            if (Projectile.localAI[0] <= 0f)
                return false;

            Vector2 bladeDirection = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
            float bladeLength = (int)Projectile.ai[0] == 3 ? 152f : 140f;
            Vector2 bladeStart = Projectile.Center + bladeDirection * 18f;
            Vector2 bladeEnd = Projectile.Center + bladeDirection * bladeLength;

            // 给剑刃一点宽度，避免细线从敌人体型边缘擦过。
            Rectangle widenedTarget = targetHitbox;
            widenedTarget.Inflate(6, 6);

            return Collision.CheckAABBvLineCollision(
                new Vector2(widenedTarget.X, widenedTarget.Y),
                new Vector2(widenedTarget.Width, widenedTarget.Height),
                bladeStart,
                bladeEnd);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (!player.GetModPlayer<MoonlightGreatswordPlayer>().MoonlightActive)
                return;

            target.AddBuff(BuffID.Frostburn, 240);
            target.AddBuff(ModContent.BuffType<Nightwither>(), 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int mode = (int)Projectile.ai[0];
            float aim = Projectile.ai[1];
            float direction = Projectile.ai[2] < 0f ? -1f : 1f;
            float maxTime = Projectile.localAI[0] > 0f ? Projectile.localAI[0] : 24f;
            float progress = MathHelper.Clamp(1f - Projectile.timeLeft / maxTime, 0f, 1f);

            GetSwingAngles(mode, aim, direction, out float startAngle, out float endAngle);

            bool buffed = player.GetModPlayer<MoonlightGreatswordPlayer>().MoonlightActive;
            float scale = mode == 3 ? 0.68f : 0.62f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color mainColor = buffed ? new Color(185, 220, 255) : Color.White;

            // 残影。
            for (int i = 4; i >= 1; i--)
            {
                float ghostProgress = MathHelper.Clamp(progress - i * 0.055f, 0f, 1f);
                float ghostAngle = MathHelper.Lerp(startAngle, endAngle, Ease(mode, ghostProgress));
                Color ghostColor = new Color(120, 170, 255) * (0.035f * (5 - i) * (buffed ? 1.5f : 0.8f));

                Main.EntitySpriteDraw(
                    texture,
                    drawPosition,
                    null,
                    ghostColor,
                    ghostAngle + MathHelper.PiOver4,
                    HiltOrigin,
                    scale * (1f - i * 0.012f),
                    SpriteEffects.None,
                    0);
            }

            // 外发光 + 剑身。
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                new Color(100, 160, 255) * 0.18f,
                Projectile.rotation,
                HiltOrigin,
                scale * 1.06f,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                mainColor,
                Projectile.rotation,
                HiltOrigin,
                scale,
                SpriteEffects.None,
                0);

            return false;
        }

        private static float Ease(int mode, float progress)
        {
            if (mode == 3)
                return MathHelper.SmoothStep(0f, 1f, progress);

            return 1f - (1f - progress) * (1f - progress);
        }

        private static void GetSwingAngles(int mode, float aim, float direction, out float startAngle, out float endAngle)
        {
            switch (mode)
            {
                case 1:
                    // 第二段：由下向上的挑斩。
                    startAngle = aim + direction * 1.15f;
                    endAngle = aim - direction * 0.95f;
                    break;
                case 2:
                    // 第三段：幅度更大的横扫。
                    startAngle = aim - direction * 1.9f;
                    endAngle = aim + direction * 1.35f;
                    break;
                case 3:
                    // 附魔重击：过肩下劈。
                    startAngle = aim - direction * 2.1f;
                    endAngle = aim + direction * 0.5f;
                    break;
                default:
                    // 第一段：斜向下劈。
                    startAngle = aim - direction * 1.3f;
                    endAngle = aim + direction * 0.85f;
                    break;
            }
        }
    }
}
