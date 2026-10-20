using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles.MinusLineProj
{
    class ApocalypseRolling : ModProjectile
    {
        // 旋转中心点偏移（可微调）
        public Vector2 RotationCenterOffset = new(0, 0);

        public override void SetDefaults()
        {
            Projectile.width = 180;  // 弹幕的宽度
            Projectile.height = 184; // 弹幕的高度
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 24; // 与武器的 useTime 一致
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1; // 使用自定义 AI
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 如果玩家死亡或停止使用武器，销毁投射物
            if (player.dead || !player.channel)
            {
                Projectile.Kill();
                return;
            }

            // 绑定到玩家位置
            Projectile.Center = player.Center;

            // 根据挥舞进度计算旋转角度
            float progress = (float)Projectile.timeLeft / Projectile.ai[0]; // ai[0] 存储初始 timeLeft
            float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, progress);

            // 设置弹幕的旋转
            Projectile.rotation = rotation;

            // 更新玩家手持武器的方向
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = rotation;

            // 处理可见性和光照
            VisibilityAndLight();
        }

        private void VisibilityAndLight()
        {
            // 添加光照效果
            Lighting.AddLight(Projectile.Center, 1.45f, 1.22f, 0.58f);
        }
    }
}