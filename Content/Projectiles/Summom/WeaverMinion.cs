using CalamityMod;
using Calamitytwinklefragment.Content.Buff.Summom;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles.Summom
{
    internal class WeaverMinion : ModProjectile
    {
        public int Variant;
        private int jumpCooldown = 0; // 跳跃冷却计时器
        private bool wasInAir = false; // 跟踪上一帧是否在空中

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            //Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 28;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.MaxUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; // 独立无敌帧
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Variant);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Variant = reader.ReadInt32();
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 初始化变体
            if (Projectile.localAI[0] == 0f)
            {
                Variant = Main.rand.Next(3);
                Projectile.netUpdate = true;
                Projectile.localAI[0] += 1f;
            }

            // 动画更新
            /*
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 5f == 4f)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame < 1) Projectile.frame = 1;
            if (Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 1;
            */
            // 召唤物状态管理
            player.AddBuff(ModContent.BuffType<WeaverMinionBuff>(), 3600);
            if (player.dead) player.GetModPlayer<CTFMinionPlayer>().weaverMinion = false;
            if (player.GetModPlayer<CTFMinionPlayer>().weaverMinion) Projectile.timeLeft = 2;

            // 减少跳跃冷却
            if (jumpCooldown > 0)
                jumpCooldown--;

            // 寻找目标
            NPC potentialTarget = Projectile.Center.MinionHoming(900f, player);

            // 改进的跳跃逻辑 - 修复空中连跳问题
            bool isGrounded = Projectile.velocity.Y == 0 && !wasInAir;
            bool shouldJump = false;

            // 只在确实在地面时检查跳跃条件
            if (isGrounded)
            {
                // 基础跳跃条件：坑洞或卡住
                if (HoleBelow() || (Projectile.Distance(player.Center) > 205f && Projectile.position.X == Projectile.oldPosition.X))
                {
                    shouldJump = true;
                }

                // 如果目标在较高位置且距离合适，主动跳跃攻击
                if (potentialTarget != null && jumpCooldown == 0)
                {
                    float verticalDistance = potentialTarget.Center.Y - Projectile.Center.Y;
                    float horizontalDistance = Math.Abs(potentialTarget.Center.X - Projectile.Center.X);

                    // 如果目标在召唤物上方且水平距离不太远，则跳跃
                    if (verticalDistance < -30f && horizontalDistance < 150f)
                    {
                        shouldJump = true;
                        jumpCooldown = 20; // 减少冷却时间，但要确保不会连跳
                    }
                }

                if (shouldJump)
                {
                    Projectile.velocity.Y = -13.5f; // 1.5倍跳跃高度
                }
            }

            // 更新空中状态跟踪
            wasInAir = Projectile.velocity.Y != 0f;

            // 重力 - 稍微降低重力加速度
            if (Projectile.velocity.Y > -16f)
            {
                Projectile.velocity.Y += 0.3f; // 降低重力
            }

            // 移动逻辑
            if (potentialTarget is null)
            {
                // 1.5倍移动速度
                if (Math.Abs(player.Center.X - Projectile.Center.X + 40f * Projectile.minionPos) > 160f)
                {
                    Projectile.velocity.X += Main.rand.NextFloat(0.16f, 0.25f) *
                        (player.Center.X - Projectile.Center.X + 40f * Projectile.minionPos > 0f).ToDirectionInt();
                    Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -20f, 20f);
                }
                else
                {
                    Projectile.velocity.X *= 0.95f;
                }

                if (Projectile.Distance(player.Center) <= 150f)
                {
                    Projectile.frame = 0;
                }

                if (Projectile.Distance(player.Center) > 1600f)
                {
                    Projectile.Center = player.Center;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                // 1.5倍追击速度
                if (Math.Abs(potentialTarget.Center.X - Projectile.Center.X + (int)MathHelper.Min(potentialTarget.width / 2, 40f)) > (int)MathHelper.Min(potentialTarget.width / 2, 40f))
                {
                    Projectile.velocity.X += Main.rand.NextFloat(0.12f, 0.225f) *
                        (potentialTarget.Center.X - Projectile.Center.X > 0f).ToDirectionInt();
                    Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -24f, 24f);
                }
                else
                {
                    Projectile.velocity.X *= 0.95f;
                }

                if (Projectile.Distance(player.Center) > 1400f)
                {
                    Projectile.Center = player.Center;
                    Projectile.netUpdate = true;
                }
            }

            // 方向设置
            if (Projectile.velocity.X > 0.25f)
                Projectile.spriteDirection = 1;
            else if (Projectile.velocity.X < -0.25f)
                Projectile.spriteDirection = -1;

            // 编织者特效：留下丝线轨迹
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Silk, 0f, 0f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.3f;
            }
        }

        public bool HoleBelow()
        {
            int tileWidth = 4;
            int tileX = (int)(Projectile.Center.X / 16f) - tileWidth;
            if (Projectile.velocity.X > 0) tileX += tileWidth;

            int tileY = (int)((Projectile.position.Y + Projectile.height) / 16f);

            for (int y = tileY; y < tileY + 2; y++)
            {
                for (int x = tileX; x < tileX + tileWidth; x++)
                {
                    if (Main.tile[x, y].HasTile) return false;
                }
            }
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Rectangle frame = new(Variant * Projectile.width, Projectile.frame * Projectile.height, Projectile.width, Projectile.height);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value,
                Projectile.Center - Main.screenPosition, frame, Color.White,
                Projectile.rotation, Projectile.Size / 2f, 1f, spriteEffects, 0);
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = Projectile.Bottom.Y < Main.player[Projectile.owner].Top.Y - 120f;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}