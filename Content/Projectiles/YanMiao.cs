using CalamityMod;
using CalamityMod.CalPlayer;
using Calamitytwinklefragment.Content.TFTools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    /// <summary>
    /// 盐喵召唤物：按血量、敌人距离与移动状态在睡觉 / 飞行 / 走路 / 等待四种模式间切换。
    /// 每种模式使用一张独立的纵向多帧贴图，帧数不同，因此绘制由 PreDraw 接管，
    /// 按当前贴图的实际高度切帧，而不是依赖单一的 Main.projFrames。
    /// </summary>
    class YanMiao : ModProjectile, ILocalizedModType
    {
        // 状态变量
        public int YanMode = 0;
        private int frameCounter = 0;
        private int currentFrame = 0;
        private bool initialized = false;
        private int currentTotalFrames; // 当前模式的总帧数
        private int lastYanMode = -1;   // 上一帧的模式，用于切模式时重置动画

        public new string LocalizationCategory => "Projectiles.Summon";
        public Player Owner => Main.player[Projectile.owner];

        public CalamityPlayer ModdedOwner => Owner.Calamity();

        public ref float CheckForSpawning => ref Projectile.localAI[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // 启用自动索敌功能

            // 飞行模式的残影需要历史位置。
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

            // 绘制完全由 PreDraw 接管；这里保留 1 帧，避免原版按整张长贴图裁切。
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 47;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1; // 永久存在
            Projectile.timeLeft = 3600; // 每帧刷新
            Projectile.DamageType = DamageClass.Generic; // 伤害类型
            Projectile.tileCollide = true; // 与方块碰撞
            Projectile.netImportant = true; // 同步到多人模式
            Projectile.spriteDirection = 1; // 贴图默认朝右
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 同步状态
            if (Projectile.localAI[0] == 0)
            {
                Projectile.netUpdate = true;
                Projectile.localAI[0] = 1;
            }

            // 检查Buff是否存在
            if (!player.HasBuff(ModContent.BuffType<Buff.YanMiaoPower>()))
            {
                Projectile.Kill();
                return;
            }

            // 状态机更新
            UpdateYanMode(player);

            // 执行对应AI
            switch (YanMode)
            {
                case 0: Mode0Behavior(player); break;
                case 1: Mode1Behavior(); break;
                case 2: Mode2Behavior(player); break;
                case 3: Mode3Behavior(); break;
            }

            // 更新动画与朝向
            UpdateAnimation();
            UpdateFacing(player);
        }

        private void UpdateYanMode(Player player)
        {
            // 强制模式0条件
            if (player.statLife <= player.statLifeMax2 / 2)
            {
                YanMode = 0;
                return;
            }

            // 模式0退出条件
            if (YanMode == 0 && player.statLife > player.statLifeMax2 * 0.8f)
            {
                // 继续检测其他条件
            }
            else if (YanMode == 0)
            {
                return;
            }

            // 检测附近敌怪
            // 获取目标 NPC
            NPC target = Projectile.Center.ClosestNPCAt(2000f, true);

            if (target != null && target.active)
            {
                YanMode = 1;
                return;
            }

            // 玩家移动检测
            bool playerMoving = player.velocity.Length() > 1f;

            if (playerMoving)
            {
                YanMode = 2;
            }
            else
            {
                // 检测玩家静止
                if (Vector2.Distance(Projectile.Center, player.Center) > 70f)
                {
                    YanMode = 2;
                }
                else
                {
                    YanMode = 3;
                }
            }
        }

        #region Mode Behaviors
        private void Mode0Behavior(Player player)
        {
            // 瞬移到头部位置
            Vector2 headPosition = player.Center + new Vector2(0, -player.height);
            Projectile.Center = headPosition;

            // 初始化动画
            if (!initialized)
            {
                currentFrame = 0;
                frameCounter = 0;
                initialized = true;
            }

            // 限制移动
            Projectile.velocity = player.velocity;
        }

        private void Mode1Behavior()
        {
            // 调用 ChargingMinionAI 实现自动索敌和攻击
            Projectile.CTFChargingMinionAI1(1500f, 1500f, 2200f, 150f, 0, 24f, 15f, 4f, new Vector2(0f, -60f), 12f, 12f, false, false, 1);
        }

        private void Mode2Behavior(Player player)
        {
            // 瞬移检测
            float maxDistance = 1000f; // 最大跟随距离
            if (Vector2.DistanceSquared(Projectile.Center, player.Center) > maxDistance * maxDistance)
            {
                // 瞬移到玩家附近
                Projectile.Center = player.Center + new Vector2(-player.direction * 40, -20);
                Projectile.velocity = Vector2.Zero;
                return;
            }
            // 目标位置（玩家附近）
            Vector2 targetPos = player.Center + new Vector2(-player.direction * 40, -20);
            Vector2 moveVector = targetPos - Projectile.Center;

            // 横向移动逻辑
            if (Math.Abs(moveVector.X) > 10f)
            {
                // 设置水平速度
                Projectile.velocity.X = Math.Sign(moveVector.X) * 3f; // 移动速度

                // 检测前方障碍物
                if (Collision.SolidCollision(Projectile.position + new Vector2(Projectile.velocity.X, 0), Projectile.width, Projectile.height))
                {
                    // 跳跃
                    Projectile.velocity.Y = -6f; // 跳跃高度
                }
            }

            // 重力模拟
            if (!Collision.SolidCollision(Projectile.position + new Vector2(0, 1), Projectile.width, Projectile.height))
            {
                Projectile.velocity.Y += 0.3f; // 重力加速度
            }
            else
            {
                Projectile.velocity.Y = 0; // 落地
            }

            // 限制垂直速度
            if (Projectile.velocity.Y > 10f)
            {
                Projectile.velocity.Y = 10f;
            }
        }

        private void Mode3Behavior()
        {
            // 静止状态
            Projectile.velocity = Vector2.Zero;
        }
        #endregion

        private void UpdateAnimation()
        {
            // 模式切换会换贴图与帧高，必须从第一帧重新开始，否则会跨贴图错位。
            if (lastYanMode != YanMode)
            {
                lastYanMode = YanMode;
                currentFrame = 0;
                frameCounter = 0;
            }

            currentTotalFrames = GetFrameCount();

            int frameSpeed = YanMode switch
            {
                0 => 5,   // 睡觉
                1 => 6,   // 飞行
                2 => 8,   // 走路
                3 => 10,  // 等待
                _ => 5
            };

            frameCounter++;
            if (frameCounter >= frameSpeed)
            {
                frameCounter = 0;
                currentFrame = (currentFrame + 1) % currentTotalFrames;
            }

            Projectile.frame = currentFrame;
        }

        /// <summary>贴图默认朝右；有水平速度时按速度转向，静止时朝向玩家所看的方向。</summary>
        private void UpdateFacing(Player player)
        {
            if (YanMode == 1 || YanMode == 2)
            {
                if (Math.Abs(Projectile.velocity.X) > 0.1f)
                    Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = player.direction;
            }
        }

        /// <summary>当前模式的帧数，与各贴图的实际高度对应。</summary>
        private int GetFrameCount()
        {
            return YanMode switch
            {
                0 => 7,   // sleep / sleep1：7 帧
                1 => 10,  // Fly：10 帧
                2 => 4,   // walk：4 帧
                3 => 8,   // Waiting：8 帧
                _ => 1
            };
        }

        public override string Texture
        {
            get
            {
                return GetCurrentTexture();
            }
        }

        private string GetCurrentTexture()
        {
            return "Calamitytwinklefragment/Content/Projectiles/YanMiao_" + GetModeSuffix();
        }

        private string GetModeSuffix()
        {
            return YanMode switch
            {
                0 => initialized ? "sleep1" : "sleep",
                1 => "Fly",
                2 => "walk",
                3 => "Waiting",
                _ => "sleep",
            };
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameCount = Math.Max(1, currentTotalFrames > 0 ? currentTotalFrames : GetFrameCount());
            int frameHeight = Math.Max(1, texture.Height / frameCount);
            int frame = Math.Clamp(currentFrame, 0, frameCount - 1);

            Rectangle source = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            float time = Main.GlobalTimeWrappedHourly;

            // 睡觉/等待时轻微呼吸，飞行时整体上下浮动；只影响绘制，不改变判定位置。
            float breathe = (YanMode == 0 || YanMode == 3) ? 1f + MathF.Sin(time * 2f) * 0.015f : 1f;
            float bob = YanMode == 1 ? MathF.Sin(time * 6f) * 2f : 0f;

            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, bob);
            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float drawScale = Projectile.scale * breathe;

            // 飞行时用历史位置画两道残影，强调扑击速度。
            if (YanMode == 1)
            {
                for (int i = 1; i <= 2; i++)
                {
                    int historyIndex = Math.Min(i * 2, Projectile.oldPos.Length - 1);
                    if (Projectile.oldPos[historyIndex] == Vector2.Zero)
                        continue;

                    Vector2 ghostPosition = Projectile.oldPos[historyIndex] + Projectile.Size / 2f
                        - Main.screenPosition + new Vector2(0f, bob);

                    Main.EntitySpriteDraw(
                        texture,
                        ghostPosition,
                        source,
                        lightColor * (0.16f / i),
                        Projectile.rotation,
                        origin,
                        drawScale,
                        effects,
                        0);
                }
            }

            Main.EntitySpriteDraw(
                texture,
                position,
                source,
                lightColor,
                Projectile.rotation,
                origin,
                drawScale,
                effects,
                0);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 弹幕与方块碰撞时的逻辑
            return false; // 返回 false 表示弹幕不会因碰撞而消失
        }
    }
}
