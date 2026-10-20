using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    public class SyzygyBowProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.alpha = 200;
            Projectile.scale = 0.8f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 如果玩家不存在或已死亡，移除弹幕
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // 跟随玩家
            Projectile.Center = player.Center + new Vector2(0, -40);
            Projectile.timeLeft = 60; // 持续刷新时间
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取月相数据
            int wang = (int)Projectile.ai[0];
            int shuo = (int)Projectile.ai[1];

            // 计算月相
            _ = CalculateMoonPhase(wang, shuo);

            // 绘制月亮
            //DrawMoon(Projectile.Center - Main.screenPosition, Color.White * 0.8f);

            return false;
        }

        private static float CalculateMoonPhase(int wang, int shuo)
        {
            // 月相计算逻辑
            if (wang == 14) return 1.0f; // 满月
            if (shuo == 14) return 0.0f; // 朔月（新月）

            // 根据望和朔的比值计算月相
            return (float)wang / (wang + shuo);
        }


    }
}