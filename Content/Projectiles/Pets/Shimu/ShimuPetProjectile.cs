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

namespace Calamitytwinklefragment.Content.Projectiles.Pets.Shimu
{
    public class ShimuPetProjectile : ModProjectile
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
                Texture2D txd = ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Shimu/ShimuPetProjectile").Value;
                Main.EntitySpriteDraw(txd, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(txd.Width, txd.Height) / 2, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
                return false;
            }

            // 加载动画贴图
            List<Texture2D> list =
            [
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Shimu/ShimuPetProjectile1").Value,
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Shimu/ShimuPetProjectile2").Value,
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Shimu/ShimuPetProjectile3").Value,
                ModContent.Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/Pets/Shimu/ShimuPetProjectile4").Value
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
            if (!player.dead && player.HasBuff(ModContent.BuffType<ShimuPetBuff>()))
            {
                Projectile.timeLeft = 2; // 无限延长存在时间
            }
        }

        private void MoveToTarget(Vector2 targetPos)
        {
            // 如果距离玩家过远，瞬移到玩家位置
            if (Vector2.Distance(Projectile.Center, targetPos) > 1400)
            {
                Projectile.Center = Main.player[Projectile.owner].Center;
            }

            // 根据水平速度设置旋转角度（模拟飞行倾斜效果）
            Projectile.rotation = MathHelper.ToRadians((Projectile.velocity.X * 1.4f));

            // 向目标位置移动
            if (Vector2.Distance(Projectile.Center, targetPos) > 34)
            {
                Vector2 px = targetPos - Projectile.Center;
                px.Normalize();
                Projectile.velocity += px * 0.84f; // 加速朝向目标
                Projectile.velocity *= 0.935f;   // 速度衰减
            }
            else
            {
                Projectile.velocity *= 0.8f; // 靠近目标时减速
            }

            // 更新方向
            Projectile.direction = (Projectile.velocity.X > 0) ? 1 : -1;
        }
    }
}