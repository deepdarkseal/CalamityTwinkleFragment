using CalamityMod;
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
    internal class ShuoProjectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 300;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; // 独立无敌帧
            Projectile.scale = 2f;
        }

        // 在类级别添加字段来跟踪冲撞状态
        private int cooldown = 0;
        private Vector2 lastVelocity;

        public override void AI()
        {
            Projectile.rotation += 0.1f;

            // 冲撞参数
            float maxSpeed = 30f;
            float accelerationForce = 1.2f;
            float friction = 0.95f;

            // 冷却时间处理（冲撞后的短暂停顿）
            if (cooldown > 0)
            {
                cooldown--;
                Projectile.velocity *= 0.8f; // 冷却期间减速更快
                goto DustEffect; // 跳过目标寻找逻辑
            }

            // 寻找目标
            NPC target = Projectile.Center.ClosestNPCAt(2000f, true);
            if (target != null && target.active)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();

                float distanceToTarget = Vector2.Distance(Projectile.Center, target.Center);

                // 根据距离调整行为
                if (distanceToTarget > 200f)
                {
                    // 远距离：平稳加速
                    Projectile.velocity += direction * accelerationForce;
                }
                else if (distanceToTarget > 50f)
                {
                    // 中距离：强力冲刺
                    Projectile.velocity += direction * accelerationForce * 2f;
                }
                else
                {
                    // 近距离：执行冲撞并进入冷却
                    Projectile.velocity = direction * maxSpeed * 1.5f;
                    cooldown = 20; // 20帧的冷却时间
                    lastVelocity = Projectile.velocity;
                }
            }
            else
            {
                // 没有目标时，逐渐减速并轻微随机移动
                Projectile.velocity *= 0.98f;
                if (Main.rand.NextBool(60))
                {
                    Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45));
                }
            }

            // 限制最大速度
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
            }

            // 应用摩擦力
            Projectile.velocity *= friction;

        DustEffect:
            // 黑暗特效
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f);
            }
        }
    }
}