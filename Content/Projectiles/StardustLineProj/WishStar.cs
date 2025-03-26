using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;
using Terraria.ID;
using CalamityMod.Particles;

namespace Calamitytwinklefragment.Content.Projectiles.StardustLineProj
{
    internal class WishStar : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6; // 拖尾缓存长度
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;     // 拖尾模式（2 为高级插值）
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1.5f;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420; // 存在时间
            Projectile.tileCollide = false;

            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 20; // 独立无敌帧
        }

        public override void AI()
        {
            // 前减速
            if (Projectile.timeLeft > 360)
            {
                Projectile.velocity *= 0.95f;
            }
            else // 开始追踪敌人
            {
                Projectile.penetrate = 1;
                NPC target = Projectile.Center.ClosestNPCAt(2000f, true);
                if (target != null && target.active)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    float starSpeed = Main.rand.NextFloat(15f, 25f);
                    Projectile.velocity = direction * starSpeed;
                }
            }

            // 添加光照
            Lighting.AddLight(Projectile.Center, 1f, 1f, 0.1f); // 黄色光

            // 生成航迹云粒子（每帧生成）
            GenerateContrailParticles();
            // 生成烟雾粒子
            if (Main.rand.NextBool(2))
            {
                // 使用更自然的颜色渐变，模拟星星的光芒
                _ = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color starColor = Main.rand.Next(3) switch
                {
                    0 => Color.Lerp(Color.White, Color.Gold, 0.3f),
                    1 => Color.Lerp(Color.SkyBlue, Color.White, 0.7f),
                    _ => Color.Lerp(Color.Purple, Color.HotPink, 0.5f)
                };

                Particle smoke = new HeavySmokeParticle(
                    position: Projectile.Center,
                    velocity: Projectile.velocity * 0.3f, // 降低速度，使粒子更柔和
                    color: starColor,
                    lifetime: 15,
                    scale: Main.rand.NextFloat(0.5f, 1.0f) * Projectile.scale,
                    opacity: 0.3f, // 降低透明度，使粒子更轻盈
                    0,
                    false,
                    0,
                    required: true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);


                //概率生成发光烟雾
                if (Main.rand.NextBool(3))
                {
                    Particle smokeGlow = new HeavySmokeParticle(
                        position: Projectile.Center,
                        velocity: Projectile.velocity * 0.2f, // 进一步降低速度
                        color: Color.SkyBlue,
                        lifetime: 30,
                        scale: Main.rand.NextFloat(0.3f, 0.6f) * Projectile.scale,
                        opacity: 0.7f, // 提高透明度，使发光更明显
                        0,
                        true,
                        0.01f, // 增加发光强度
                        required: true
                    );
                    GeneralParticleHandler.SpawnParticle(smokeGlow);
                }
            }
        }

        // 生成航迹云粒子的独立方法
        private void GenerateContrailParticles()
        {
            Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            if (normalizedVelocity == Vector2.Zero)
                return;

            // 计算两侧偏移方向
            Vector2 perpendicular = new(-normalizedVelocity.Y, normalizedVelocity.X);            

            // 生成一个 0 到 2 的随机整数
            int randomValue = Main.rand.Next(2); // 0, 1, 2

            float offsetDistance;
            if (Main.rand.NextBool(8))
            {
                for (int j = 0; j < 2; j++)
                {
                    offsetDistance = Main.rand.NextFloat(5f, 7f); // 粒子与弹幕中心的偏移距离
                    Vector2 spawnPosition = Projectile.Center + perpendicular * (j == 0 ? offsetDistance : -offsetDistance);
                    // 补充Dust粒子增强效果
                    Dust contrailDust = Dust.NewDustPerfect(
                        spawnPosition,
                        DustID.SnowflakeIce,
                        Projectile.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-1f, 1f)),
                        Main.rand.Next(5, 15),
                        Color.CornflowerBlue,
                        Main.rand.NextFloat(0.4f, 0.7f)
                    );
                    contrailDust.noGravity = true;
                    contrailDust.fadeIn = 1.2f;
                }
            }
            if (Main.rand.NextBool(4))
            {
                for (int ij = 0; ij < 2; ij++)
                {
                    offsetDistance = Main.rand.NextFloat(5f, 7f); // 粒子与弹幕中心的偏移距离
                    Vector2 spawnPosition = Projectile.Center + perpendicular * (ij == 0 ? offsetDistance : -offsetDistance);
                    // 补充Dust粒子增强效果
                    Dust contrailDust = Dust.NewDustPerfect(
                        spawnPosition,
                        DustID.GoldFlame,
                        Projectile.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-1f, 1f)),
                        Main.rand.Next(5, 15),
                        Color.Gold,
                        Main.rand.NextFloat(0.4f, 0.7f)
                    );
                    contrailDust.noGravity = true;
                    contrailDust.fadeIn = 1.2f;
                }
            }
        }       
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