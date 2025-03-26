using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using MonoMod.Core.Utils;
using CalamityMod;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace Calamitytwinklefragment.Content.Projectiles
{
    class YanMiao : ModProjectile
    {
        // 状态变量
        public int YanMode = 0;
        private int frameCounter = 0;
        private int currentFrame = 0;
        private bool initialized = false;
        private int currentTotalFrames = 1; // 当前模式的总帧数
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // 启用自动索敌功能
        }
        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1; // 永久存在
            Projectile.timeLeft = 3600; // 每帧刷新
            Projectile.DamageType = DamageClass.Generic; // 伤害类型
            Projectile.tileCollide = true; // 与方块碰撞
            Projectile.netImportant = true; // 同步到多人模式
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
                case 1: Mode1Behavior(player); break;
                case 2: Mode2Behavior(player); break;
                case 3: Mode3Behavior(player); break;
            }

            // 更新动画
            UpdateAnimation();
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

        private void Mode1Behavior(Player player)
        {
            // 调用 ChargingMinionAI 实现自动索敌和攻击
            Projectile.ChargingMinionAI(1500f, 1500f, 2200f, 150f, 0, 24f, 15f, 4f, new Vector2(0f, -60f), 12f, 12f, false, false, 1);
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

        private void Mode3Behavior(Player player)
        {
            // 静止状态
            Projectile.velocity = Vector2.Zero;
        }
        #endregion

        private void UpdateAnimation()
        {
            frameCounter++;

            int frameSpeed;
            int totalFrames;

            switch (YanMode)
            {
                case 0:
                    frameSpeed = 5;
                    totalFrames = 7;
                    break;
                case 1:
                    frameSpeed = 6;
                    totalFrames = 10;
                    break;
                case 2:
                    frameSpeed = 8;
                    totalFrames = 4;
                    break;
                case 3:
                    frameSpeed = 10;
                    totalFrames = 8;
                    break;
                default:
                    frameSpeed = 5;
                    totalFrames = 1;
                    break;
            }

            // 动态更新总帧数
            currentTotalFrames = totalFrames;

            if (frameCounter >= frameSpeed)
            {
                frameCounter = 0;
                currentFrame = (currentFrame + 1) % currentTotalFrames;
            }

            Projectile.frame = currentFrame;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // 按当前模式的总帧数分割贴图
            int singleFrameHeight = tex.Height / currentTotalFrames;
            Rectangle frameRect = new(0, singleFrameHeight * currentFrame, tex.Width, singleFrameHeight);

            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Vector2 origin = new(tex.Width / 2f, singleFrameHeight / 2f);

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 按弹幕实际尺寸缩放绘制
            float scaleX = Projectile.width / (float)tex.Width;
            float scaleY = Projectile.height / (float)singleFrameHeight;
            Vector2 scale = new Vector2(scaleX, scaleY) * Projectile.scale;

            Main.EntitySpriteDraw(
                tex,
                drawPos,
                frameRect,
                lightColor,
                Projectile.rotation,
                origin,
                scale, // 使用计算后的缩放比例
                effects,
                0
            );

            return false;
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
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 弹幕与方块碰撞时的逻辑
            return false; // 返回 false 表示弹幕不会因碰撞而消失
        }
    }
}
