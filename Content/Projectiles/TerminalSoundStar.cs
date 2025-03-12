using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class TerminalSoundStar : ModProjectile
    {

        // 初始偏移方向（左或右）
        private Vector2 perpendicularDirection;

        // 随机数生成器
        private readonly Random random = new();

        // 力的强度范围
        private readonly float minForce = 0f;
        private readonly float maxForce = 2.1f;

        // 颜色参数
        public ref float Hue => ref Projectile.ai[0]; // 通过 ai[0] 控制颜色

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6; // 拖尾缓存长度
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;     // 拖尾模式（2 为高级插值）
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.penetrate = 77;
            Projectile.timeLeft = 210;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;

            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 49; // 独立无敌帧
        }

        public override void AI()
        {
            // 初始化垂直方向
            if (perpendicularDirection == Vector2.Zero)
                InitializePerpendicularDirection();

            // 施加随机力
            ApplyRandomForce();

            // 更新弹幕速度与位置
            UpdateVelocity();

            // 限制速度
            LimitVelocity();

            // 更新旋转
            UpdateRotation();

            // 添加光照
            Lighting.AddLight(Projectile.Center, 0.75f, 1f, 0.24f); // 黄绿色光

            // 生成烟雾粒子
            if (Main.rand.NextBool(2))
            {
                Particle smoke = new HeavySmokeParticle(
                    position: Projectile.Center,
                    velocity: Projectile.velocity * 0.5f,
                    color: Color.Lerp(Color.MidnightBlue, Color.Indigo, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f)),
                    lifetime: 30,
                    scale: Main.rand.NextFloat(0.6f, 1.2f) * Projectile.scale,
                    opacity: 0.8f,
                    0,
                    false,
                    0,
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smoke);

                // 33% 概率生成发光烟雾
                if (Main.rand.NextBool(3))
                {
                    Particle smokeGlow = new HeavySmokeParticle(
                        position: Projectile.Center,
                        velocity: Projectile.velocity * 0.5f,
                        color: Main.hslToRgb(Hue, 1, 0.7f),
                        lifetime: 20,
                        scale: Main.rand.NextFloat(0.4f, 0.7f) * Projectile.scale,
                        opacity: 0.8f,
                        0,
                        true,
                        0.005f,
                        required: true
                    );
                    GeneralParticleHandler.SpawnParticle(smokeGlow);
                }
            }
        }

        //--- 核心逻辑方法 ---
        private void InitializePerpendicularDirection()
        {
            Vector2 velocityDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float angle = random.Next(2) == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
            perpendicularDirection = velocityDirection.RotatedBy(angle);
        }

        private void ApplyRandomForce()
        {
            float forceMagnitude = (float)(random.NextDouble() * (maxForce - minForce) + minForce);
            float angleVariation = (float)(random.NextDouble() * MathHelper.Pi - MathHelper.PiOver2);
            Vector2 forceDirection = perpendicularDirection.RotatedBy(angleVariation);
            Projectile.velocity += forceDirection * forceMagnitude;
        }

        private void UpdateVelocity() => Projectile.position += Projectile.velocity;

        private void LimitVelocity()
        {
            float maxSpeed = 21f;
            if (Projectile.velocity.Length() > maxSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
        }

        private void UpdateRotation() => Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        //--- 自定义绘制 ---
        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制拖尾效果
            CalamityUtils.DrawAfterimagesCentered(
                Projectile,
                ProjectileID.Sets.TrailingMode[Projectile.type],
                lightColor,
                1
            );
            return false; // 禁用默认绘制
        }
    }
}