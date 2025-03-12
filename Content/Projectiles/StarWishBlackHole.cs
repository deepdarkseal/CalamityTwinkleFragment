using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Projectiles.Magic;
using Elasticsearch.Net;
using Microsoft.Xna.Framework.Graphics;
using System;
using Mono.Cecil;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class StarWishBlackHole : ModProjectile
    {
        private List<AccretionDiskParticle> _particles;
        public override void SetStaticDefaults()
        {
        }
        public float RotationSpeed; // 粒子的旋转速度


        public override void SetDefaults()
        {
            Projectile.width = 240;
            Projectile.height = 180;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 480; // 存在时间（8秒）
            Projectile.tileCollide = false;
            //Projectile.hide = true; // 隐藏默认弹幕
            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 10; // 独立无敌帧
        }
        
        public override void AI()
        {
            // 初始化粒子
            if (_particles == null)
            {
                _particles = [];
                int particleCount = 300; // 粒子数量
                for (int i = 0; i < particleCount; i++)
                {
                    float radius = Main.rand.NextFloat(80f, 95f); // 随机半径
                    float angle = MathHelper.TwoPi * i / particleCount; // 均匀分布角度
                    float size = Main.rand.NextFloat(2f, 8f);            // 粒子大小
                    RotationSpeed = Main.rand.NextFloat(0.01f, 0.03f); // 随机旋转速度
                    Color color = GetColorAtRadius(radius);              // 根据半径获取颜色
                    _particles.Add(new AccretionDiskParticle(Projectile.Center, radius, angle, size, color));
                }
            }

            // 更新粒子
            foreach (var particle in _particles)
            {
                particle.UpdateAngle(); // 使用粒子的随机旋转速度
                particle.UpdatePosition(Projectile.Center);
            }

            // 跟随鼠标移动
            _ = Main.player[Projectile.owner];
            Projectile.Center = Main.MouseWorld;

            bool isBossAlive = false;
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f); // 发光
            foreach (NPC npc in Main.npc)
            {
                if (npc.active)
                {
                    if (npc.boss)
                    {
                        isBossAlive = true;
                        break; // 如果发现 Boss，直接跳出循环
                    }

                    // 如果没有 Boss 存活，则检查范围内的非友好 NPC
                    if (!isBossAlive && !npc.friendly && npc.Distance(Projectile.Center) < 600)
                    {
                        Vector2 direction = Projectile.Center - npc.Center;
                        float distance = direction.Length();

                        if (distance > 0)
                        {
                            Vector2 forceDirection = direction / distance;  // 归一化方向向量（单位向量）
                            float forceStrength = 600 / distance* distance;          // 假设力的大小随距离减小

                            // 应用拉力到速度
                            Vector2 forceVector = forceDirection * forceStrength;

                            // 调整 NPC 的 velocity
                            npc.velocity += forceVector;

                            // 限制最大速度，防止 NPC 无限加速
                            Vector2 currentVel = npc.velocity;
                            if (currentVel.Length() > maxSpeed)
                            {
                                currentVel.Normalize();
                                currentVel *= maxSpeed;
                                npc.velocity = currentVel;
                            }
                        }
                    }
                }
            }


            // 爆炸效果
            if (Projectile.timeLeft <= 30)
            {
                Projectile.alpha += 10; // 逐渐消失
                if (Projectile.timeLeft == 1)
                {
                    Vector2 mouseWorldPosition = Main.MouseWorld;
                    float mouseX = mouseWorldPosition.X; // 鼠标的 X 坐标
                    float mouseY = mouseWorldPosition.Y; // 鼠标的 Y 坐标

                    for (int i = 0; i < 12; i++)
                    {
                        // 随机生成x坐标
                        float x = mouseX + Main.rand.NextFloat(-150f, 150f);

                        // 随机生成y坐标
                        float y = mouseY + Main.rand.NextFloat(-150f, 150f);

                        // 发射弹幕到指定位置
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(x, y), Vector2.Zero,
                        ModContent.ProjectileType<NebulaNova>(), Projectile.damage * 3, Projectile.knockBack, Projectile.owner);
                        
                        Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        new Vector2(x, y),
                        Vector2.Zero,
                        ModContent.ProjectileType<WishStar>(),
                        (int)(Projectile.damage * 1),
                        Projectile.knockBack, Projectile.owner);
                    }
                }
            }
        }
        // 示例：定义最大速度（根据游戏需求调整）
        private const float maxSpeed = 30f;
        public override void PostDraw(Color lightColor)
        {
            // 绘制粒子
            foreach (var particle in _particles)
            {
                Main.spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value, // 使用一个简单的像素贴图
                    particle.Position - Main.screenPosition,
                    new Rectangle(0, 0, 1, 1),
                    particle.Color,
                    0f,
                    Vector2.One * 0.5f,
                    particle.Size,
                    SpriteEffects.None,
                    0f
                );
            }

            return;
        }
        // 根据半径获取颜色（白→黄→橙）
        private static Color GetColorAtRadius(float radius)
        {
            if (radius < 85f)
                return Color.White;
            else if (radius < 88f)
                return Color.Yellow;
            else
                return Color.Orange;
        }
        public class AccretionDiskParticle
        {
            public Vector2 Position; // 粒子的位置
            public float Radius;     // 粒子绕弹幕中心的半径
            public float Angle;       // 粒子当前的角度
            public float Size;        // 粒子的大小
            public Color Color;       // 粒子的颜色
            public float RotationSpeed; // 粒子的旋转速度

            public AccretionDiskParticle(Vector2 center, float radius, float angle, float size, Color color)
            {
                Radius = radius;
                Angle = angle;
                Size = size;
                Color = color;
                RotationSpeed = Main.rand.NextFloat(0.1f, 0.3f); // 随机旋转速度
                UpdatePosition(center); // 初始化位置
            }

            // 更新粒子的位置
            public void UpdatePosition(Vector2 center)
            {
                Position = center + new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle)) * Radius;
            }

            // 更新粒子的角度（旋转）
            public void UpdateAngle()
            {
                Angle += RotationSpeed; // 使用随机旋转速度
                if (Angle > MathHelper.TwoPi) // 防止角度过大
                    Angle -= MathHelper.TwoPi;
            }
        }
    }
}
