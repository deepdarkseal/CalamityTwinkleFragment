using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles.MinusLineProj
{
    class EtherOrb : ModProjectile
    {
        private readonly float _rotationSpeed = 0.1f; // 控制转向速度
        private readonly float _maxSpeed = 12f; // 最大追踪速度
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1; // 自定义 AI
            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 10; // 独立无敌帧
        }

        public override void AI()
        {
            // 生成烟雾
            if (Main.rand.NextBool(2)) // 控制烟雾生成频率
            {
                Particle smokeGlow = new HeavySmokeParticle(
                    position: Projectile.Center,
                    velocity: Projectile.velocity * 0.2f,
                    color: Color.Violet,
                    lifetime: 30,
                    scale: Main.rand.NextFloat(0.3f, 0.6f),
                    opacity: 0.6f,
                    0,
                    true,
                    0.01f,
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smokeGlow);
            }

            int minusringtime = (int)Main.rand.NextFloat(100f, 140f);
            //环绕玩家飞行
            if (Projectile.timeLeft > minusringtime)
            {
                Player player = Main.player[Projectile.owner];

                // 椭圆轨道参数
                float semiMajorAxis = 80f; // 长轴半径
                float semiMinorAxis = 60f; // 短轴半径
                float orbitSpeed = 0.05f; // 环绕速度
                float angle = Projectile.ai[0] * orbitSpeed; // 当前角度

                // 计算椭圆轨道上的位置
                Vector2 offset = new (
                    semiMajorAxis * MathF.Cos(angle),
                    semiMinorAxis * MathF.Sin(angle)
                );

                // 将偏移量旋转到玩家的当前方向
                offset = offset.RotatedBy(player.fullRotation);

                // 设置弹幕位置
                Projectile.Center = player.Center + offset;

                // 动态调整速度（近点更快，远点更慢）
                float distanceToPlayer = offset.Length();
                float speedMultiplier = MathHelper.Lerp(1.5f, 0.5f, distanceToPlayer / semiMajorAxis);
                Projectile.velocity = offset.SafeNormalize(Vector2.Zero) * speedMultiplier * 4f;

                // 更新角度
                Projectile.ai[0]++;
            }
            // 3 秒后追踪敌人
            else
            {
                NPC target = FindClosestNPC();
                if (target != null)
                {
                    // 计算目标方向
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                    // 使用线性插值平滑调整速度方向
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * _maxSpeed, _rotationSpeed);

                    // 限制速度不超过最大值
                    if (Projectile.velocity.Length() > _maxSpeed)
                    {
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * _maxSpeed;
                    }
                }
            }
        }

        private NPC FindClosestNPC()
        {
            NPC closestNPC = null;
            float closestDistance = float.MaxValue;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && npc.Distance(Projectile.Center) < closestDistance)
                {
                    closestDistance = npc.Distance(Projectile.Center);
                    closestNPC = npc;
                }
            }
            return closestNPC;
        }
    }
}
