using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class CMoonFlailProjectile : ModProjectile
    {
        // 使用类似DragonPowFlail的简单状态管理
        // ai[0] = 连枷当前行为状态
        // 0 = 连枷向外飞行
        // 1 = 连枷返回玩家
        // ai[1] = 帧计数器，用于生成弹幕和控制特效
        private float pullForce = 5f; // 拉力强度
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2; // 更平滑的移动
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = -1; // 只击中一次，让分裂弹幕也能造成伤害
            Projectile.alpha = 255; // 初始完全透明
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 更新旋转，使其指向玩家
            Vector2 posDiff = owner.Center - Projectile.Center;
            Projectile.rotation = posDiff.ToRotation();

            // 如果玩家死亡，立即销毁
            if (owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 让玩家面向连枷方向
            if (posDiff.X < 0f)
            {
                owner.ChangeDir(1);
                Projectile.direction = 1;
                Projectile.spriteDirection = 1;
                Projectile.rotation += MathHelper.Pi; // 保持连枷面朝正确方向
            }
            else
            {
                owner.ChangeDir(-1);
                Projectile.direction = -1;
                Projectile.spriteDirection = -1;
            }

            // 保持玩家手臂伸出状态
            owner.itemRotation = (-1f * posDiff * Projectile.direction).ToRotation();
            owner.itemAnimation = 6;
            owner.itemTime = 6;

            // 状态0：向外飞行
            if (Projectile.ai[0] == 0f)
            {
                // 计算当前速度
                float currentSpeed = Projectile.velocity.Length();

                // 如果还有初始速度，施加空气阻力减速
                if (currentSpeed > 5f)
                {
                    // 使用平方衰减，模拟空气阻力
                    float dragFactor = 0.97f + (1f - (currentSpeed / 25f)) * 0.03f; // 速度越慢，阻力越小
                    Projectile.velocity *= dragFactor;
                }
                else
                {
                    // 速度很低时，逐渐停止
                    Projectile.velocity *= 0.7f;
                }

                // 如果距离太远，开始返回
                float distanceToPlayer = posDiff.Length();
                if (distanceToPlayer > 800f) // 最大飞行距离800像素
                {
                    Projectile.ai[0] = 1f;
                    Projectile.netUpdate = true;

                    // 切换到返回状态时，记录当前速度方向，用于初始返回
                    Projectile.localAI[0] = Projectile.velocity.X;
                    Projectile.localAI[1] = Projectile.velocity.Y;
                }
            }
            // 状态1：返回玩家
            else
            {
                // 如果太远，直接销毁
                float distanceToPlayer = posDiff.Length();
                if (distanceToPlayer > 1500f)
                {
                    Projectile.Kill();
                    return;
                }

                // 计算指向玩家的方向
                Vector2 directionToPlayer = posDiff.SafeNormalize(Vector2.Zero);

                // 当前速度
                _ = Projectile.velocity.Length();

                // 如果还没有开始加速（刚切换状态）
                if (Projectile.localAI[0] != 0f || Projectile.localAI[1] != 0f)
                {
                    float lerpFactor = 0;
                    // 使用之前的速度方向，逐渐转向玩家
                    Vector2 initialVelocity = new(Projectile.localAI[0], Projectile.localAI[1]);
                    if (initialVelocity.Length() > 0.1f)
                    {
                        // 线性插值转向玩家方向
                        lerpFactor = MathHelper.Clamp(Projectile.ai[1] * 0.01f, 0f, 1f);
                        directionToPlayer = Vector2.Lerp(initialVelocity.SafeNormalize(Vector2.Zero), directionToPlayer, lerpFactor);
                    }

                    // 如果已经转向完成，清空初始速度
                    if (lerpFactor >= 1f)
                    {
                        Projectile.localAI[0] = 0f;
                        Projectile.localAI[1] = 0f;
                    }
                }

                // 计算需要的拉力（距离越远，拉力越大）
                float pullStrength = MathHelper.Clamp(distanceToPlayer / 400f, 0.5f, 3f);

                // 计算拉力加速度
                float acceleration = 0.5f * pullStrength;

                // 应用拉力（加速飞向玩家）
                Projectile.velocity += directionToPlayer * acceleration;

                // 限制最大返回速度
                if (Projectile.velocity.Length() > 35f)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 35f;
                }
                // 非常接近时销毁
                if (distanceToPlayer < 15f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            // 尘雾
            for (int i = 0; i < 3; i++)
            {
                int dustType = Main.rand.NextBool(3) ? DustID.Silver : DustID.WhiteTorch;
                float scale = 0.8f + Main.rand.NextFloat(0.6f);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = scale;
                Main.dust[dust].velocity *= 1.5f;
                Main.dust[dust].velocity += Projectile.velocity * 0.3f;
            }

            // 帧计数器递增
            Projectile.ai[1]++;

            // 如果连枷已经存在至少5帧，使其可见
            Projectile.alpha = (Projectile.ai[1] > 5f) ? 0 : 255;

            // 定期生成小型月亮弹幕（类似猪鲨链球泡泡）
            /*
            if (Projectile.ai[1] % 12 == 0 && Projectile.ai[0] == 0f) // 每12帧一次，只在向外飞行时生成
            {
                // 生成2-3个小型月亮弹幕
                int numProjectiles = Main.rand.Next(2, 4);
                for (int i = 0; i < numProjectiles; i++)
                {
                    Vector2 velocity = new Vector2(4f, 0f).RotatedByRandom(MathHelper.TwoPi);
                    velocity += Projectile.velocity * 0.05f;

                    if (Projectile.owner == Main.myPlayer)
                    {
                        int proj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            velocity,
                            ModContent.ProjectileType<SmallMoonProjectile>(),
                            (int)(Projectile.damage * 0.3f), // 30%伤害
                            2f,
                            Projectile.owner
                        );

                        if (proj != Main.maxProjectiles)
                        {
                            Main.projectile[proj].DamageType = DamageClass.Melee;
                            Main.projectile[proj].friendly = true;
                            Main.projectile[proj].timeLeft = 120; // 2秒存在时间
                            Main.projectile[proj].penetrate = 1;
                        }
                    }
                }

                SoundEngine.PlaySound(SoundID.Item86, Projectile.Center); // 泡泡音效
            }
            */
            // 旋转效果
            Projectile.rotation += 0.1f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 只有在距离玩家足够远时（超过40%最大距离）才触发特殊效果
            Player owner = Main.player[Projectile.owner];
            if (owner.WithinRange(target.Center, 320f)) // 320 = 800 * 0.4
                return;

            // 命中后开始返回
            Projectile.ai[0] = 1f;
            Projectile.netUpdate = true;

            // 计算从敌人指向玩家的方向
            Vector2 directionToPlayer = owner.Center - target.Center;
            float distanceToPlayer = directionToPlayer.Length();

            // 将敌人拉向玩家
            PullEnemyToPlayer(target, owner, distanceToPlayer);

            // 命中音效
            SoundEngine.PlaySound(SoundID.Item14, target.Center);
        }
        private void PullEnemyToPlayer(NPC target, Player player, float distance)
        {
            // 计算拉力方向（从敌人指向玩家）
            Vector2 pullDirection = player.Center - target.Center;
            pullDirection.Normalize();

            // 根据距离调整拉力强度（距离越远，拉力越大）
            float forceMultiplier = MathHelper.Clamp(distance / 200f, 0.5f, 3f);
            float actualPullForce = pullForce * forceMultiplier;

            // 敌人的击退抗性会影响拉力效果
            float knockbackResistance = 1f - target.knockBackResist;
            if (knockbackResistance < 0) knockbackResistance = 0;

            // 计算实际拉力
            Vector2 pullVelocity = pullDirection * actualPullForce * knockbackResistance;

            // 应用拉力到敌人的速度
            target.velocity += pullVelocity;

            // 限制敌人的最大速度，避免被拉得过快
            float maxPullSpeed = 15f;
            if (target.velocity.Length() > maxPullSpeed)
            {
                target.velocity = Vector2.Normalize(target.velocity) * maxPullSpeed;
            }

            // 添加视觉效果：敌人被拉向玩家时的特效
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.SparksMech, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = pullDirection * 3f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // 添加音效：拉拽音效
            SoundEngine.PlaySound(SoundID.Item76 with { Volume = 0.5f }, target.Center);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 碰到物块时开始返回
            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[0] = 1f;
                Projectile.netUpdate = true;

                // 碰撞音效
                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);

                // 碰撞时也生成弹幕
                //MoonExplosion(Projectile.Center);
            }
            return false;
        }

        // 绘制链子（在连枷下方绘制，避免视觉错乱）
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 mountedCenter = owner.MountedCenter;
            Texture2D chainTex = Terraria.GameContent.TextureAssets.Chain23.Value; // 使用原版链子贴图
            Vector2 chainDrawPos = Projectile.Center;

            Rectangle? sourceRectangle = null;
            Vector2 origin = new(chainTex.Width * 0.5f, chainTex.Height * 0.5f);

            Vector2 posDiff = mountedCenter - chainDrawPos;
            float rot = (float)Math.Atan2(posDiff.Y, posDiff.X) - MathHelper.PiOver2;

            // 如果向右，翻转链节使其朝上
            if (posDiff.X < 0f)
                rot += MathHelper.Pi;

            // 防止NaN导致崩溃
            bool keepDrawing = true;
            if (chainDrawPos.HasNaNs() || posDiff.HasNaNs())
                keepDrawing = false;

            // 绘制链节直到接近玩家
            while (keepDrawing)
            {
                // 当距离小于一个链节长度时停止绘制
                if (posDiff.Length() < chainTex.Height + 1f)
                    break;

                Vector2 chainDirection = posDiff.SafeNormalize(Vector2.Zero);
                chainDrawPos += chainDirection * chainTex.Height;
                posDiff = mountedCenter - chainDrawPos;
                Color colorAtLoc = Lighting.GetColor((int)chainDrawPos.X / 16, (int)chainDrawPos.Y / 16);

                Main.EntitySpriteDraw(
                    chainTex,
                    chainDrawPos - Main.screenPosition,
                    sourceRectangle,
                    colorAtLoc,
                    rot,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0
                );
            }

            // 绘制连枷本体
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            // 绘制拖尾
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / Projectile.oldPos.Length;
                Color color = Color.Lerp(Color.White * 0.5f, Color.Transparent, progress);
                float scale = Projectile.scale * (1f - progress * 0.5f);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null,
                    color,
                    Projectile.rotation,
                    texture.Size() / 2f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // 绘制连枷主体
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White,
                Projectile.rotation,
                texture.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false; // 返回false表示已经绘制了连枷本体
        }
    }

    // 小型月亮弹幕
    /*
    public class SmallMoonProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            // 微弱的跟踪能力
            if (Projectile.timeLeft > 30)
            {
                NPC target = Projectile.Center.ClosestNPCAt(200f, true);
                if (target != null && target.active)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = (Projectile.velocity * 10f + direction * 1f) / 11f;
                }
            }

            // 限制速度
            if (Projectile.velocity.Length() > 8f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8f;

            // 月光特效
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Enchanted_Gold, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }
    }
    */
}