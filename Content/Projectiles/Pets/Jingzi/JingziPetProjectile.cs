using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calamitytwinklefragment.Content.Buff.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace Calamitytwinklefragment.Content.Projectiles.Pets.Jingzi
{
    internal class JingziPetProjectile : ModProjectile
    {
        public int counter = 0; // 动画计数器

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1; // 单帧动画
            Main.projPet[Projectile.type] = true; // 标记为宠物
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish); // 克隆原版宠物属性
            Projectile.aiStyle = -1; // 禁用原版AI
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.width = 45; // 宠物宽度
            Projectile.height = 83; // 宠物高度
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 游戏菜单中的绘制逻辑（简化）
            if (Main.gameMenu)
            {
                Texture2D txd = ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Jingzi/JingziPetProjectile").Value;
                Main.EntitySpriteDraw(txd, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(txd.Width, txd.Height) / 2, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
                return false;
            }

            // 加载动画贴图
            List<Texture2D> list =
            [
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Jingzi/JingziPetProjectile1").Value,
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Jingzi/JingziPetProjectile2").Value,
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Jingzi/JingziPetProjectile3").Value,
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Jingzi/JingziPetProjectile4").Value
            ];

            // 根据计数器选择当前帧
            Texture2D tx = list[counter / 6 % list.Count];

            // 根据方向绘制贴图（水平翻转）
            if (Projectile.velocity.X > -2 && Projectile.velocity.X < 2f)
            {
                if (Main.player[Projectile.owner].Center.X > Projectile.Center.X)
                {
                    Projectile.direction = 1;
                }
                else
                {
                    Projectile.direction = -1;
                }
            }

            if (Projectile.direction == -1)
            {
                Main.EntitySpriteDraw(tx, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(tx.Width, tx.Height) / 2, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            }
            else
            {
                Main.EntitySpriteDraw(tx, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(tx.Width, tx.Height) / 2, Projectile.scale, SpriteEffects.None, 0);
            }

            return false; // 禁用原版绘制
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            player.zephyrfish = false; // 禁用原版宠物效果
            return true;
        }

        public override void AI()
        {
            counter++; // 更新动画计数器
            Player player = Main.player[Projectile.owner];

            // 移动到玩家上方偏移位置
            MoveToTarget(player.Center + new Vector2(0, -60) + new Vector2(-30 * player.direction, 0));

            // 检查玩家 Buff 以维持宠物存在
            if (!player.dead && player.HasBuff(ModContent.BuffType<JingziPetBuff>()))
            {
                Projectile.timeLeft = 2; // 无限延长存在时间
            }
        }

        private void MoveToTarget(Vector2 targetPos)
        {
            // 如果宠物距离目标位置过远，瞬移到玩家附近
            if (Vector2.Distance(Projectile.Center, targetPos) > 1400)
            {
                Projectile.Center = Main.player[Projectile.owner].Center - new Vector2(0, 50); // 瞬移到玩家上方50像素处
            }

            // 如果宠物处于飞行状态（Projectile.ai[1] == 1）
            if (Projectile.ai[1] == 1)
            {
                counter++; // 更新动画计数器
                Projectile.tileCollide = false; // 不与物块碰撞
                Projectile.rotation = MathHelper.ToRadians((Projectile.velocity.X * 1f)); // 根据水平速度设置旋转角度

                // 如果宠物距离目标位置较远，向目标移动
                if (Vector2.Distance(Projectile.Center, targetPos) > 90)
                {
                    Vector2 px = targetPos - Projectile.Center; // 计算目标方向
                    px.Normalize(); // 归一化方向向量
                    Projectile.velocity += px * 1.4f; // 加速朝向目标
                    Projectile.velocity *= 0.97f; // 速度衰减
                }

                // 如果宠物接近目标位置且玩家在地面上，切换到地面状态
                if (Projectile.Center.Y < targetPos.Y - 16 && Vector2.Distance(Projectile.Center, targetPos) < 100 && !Collision.SolidCollision(Main.player[Projectile.owner].Center + new Vector2(0, Main.player[Projectile.owner].height / 2 + 2), 1, 1))
                {
                    Projectile.ai[1] = 0; // 切换到地面状态
                }

                // 根据水平速度设置方向
                if (Projectile.velocity.X > 0)
                {
                    Projectile.direction = 1; // 向右
                }
                else
                {
                    Projectile.direction = -1; // 向左
                }
            }
            else // 如果宠物处于地面状态
            {
                // 如果宠物在地面上，根据水平速度更新动画计数器
                if (Projectile.velocity.Y == 0)
                {
                    counter += (int)Math.Abs(Projectile.velocity.X / 4); // 根据水平速度更新动画帧
                }

                Projectile.tileCollide = true; // 允许与物块碰撞
                Projectile.rotation = 0; // 重置旋转角度
                Projectile.velocity.Y += 1f; // 模拟重力

                // 检查玩家是否在地面
                if (Vector2.Distance(targetPos, Projectile.Center) > 340 || (Math.Abs(targetPos.Y - Projectile.Center.Y) > 60 && Main.player[Projectile.owner].velocity.Y == 0))
                {
                    Projectile.ai[1] = 1; // 切换到飞行状态
                }
                // 如果宠物在水平方向上距离目标较远，向目标移动
                else if (Vector2.Distance(new Vector2(targetPos.X, 0), new Vector2(Projectile.Center.X, 0)) > 80)
                {
                    if (targetPos.X > Projectile.Center.X)
                    {
                        Projectile.velocity.X += 1f; // 向右加速
                    }
                    else
                    {
                        Projectile.velocity.X -= 1f; // 向左加速
                    }
                    Projectile.velocity.X *= 0.95f; // 速度衰减
                }
                else
                {
                    Projectile.velocity.X *= 0.9f; // 减速
                }

                // 根据目标位置设置方向
                if (targetPos.X > Projectile.Center.X)
                {
                    Projectile.direction = 1; // 向右
                }
                else
                {
                    Projectile.direction = -1; // 向左
                }

                // 如果宠物在地面上且有水平速度，尝试跳跃
                if (Math.Abs(Projectile.velocity.X) > 0.3f && !Collision.SolidCollision(Projectile.Center + (Projectile.velocity * new Vector2(1, 0)).SafeNormalize(Vector2.Zero) * 14 + new Vector2(0, 23), 1, 1))
                {
                    Projectile.velocity.Y -= 1.5f; // 向上跳跃
                }
            }
        }
    }
}
