using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace Calamitytwinklefragment.Content.Items.Accessories.Mana
{
    class AstrumTelescopePlayer : ModPlayer
    {
        public bool AstrumTelescopeEquipped = false;
        private int manaGained = 0;

        public override void ResetEffects()
        {
            AstrumTelescopeEquipped = false;
        }

        // 监听魔力消耗事件
        public override void OnConsumeMana(Item item, int manaConsumed)
        {
            if (AstrumTelescopeEquipped)
            {
                manaGained = manaConsumed + manaGained;
                // 每20点魔力发射一个星星
                int starCount = manaGained / 20;
                if (starCount > 0)
                {
                    ShootStars(starCount);
                    manaGained -= starCount * 20;
                }
            }
        }
        // 发射星星弹幕
        private void ShootStars(int starCount)
        {
            Player player = Player;
            // 获取玩家的魔法伤害倍率
            float magicDamage = player.GetDamage(DamageClass.Magic).Additive;
            for (int i = 0; i < starCount; i++)
            {
                Vector2 velocity = player.DirectionTo(Main.MouseWorld) * 15f;

                // 发射星星弹幕
                int projectile = Projectile.NewProjectile(
                    player.GetSource_Accessory(Player.HeldItem), // 弹幕来源
                    player.Center, // 发射位置
                    velocity, // 速度
                    ProjectileID.SuperStar, // 弹幕类型
                    (int)(500 * magicDamage), // 伤害
                    2f, // 击退
                    player.whoAmI // 玩家索引
                );
                // 设置弹幕的伤害类型为魔法伤害
                Main.projectile[projectile].DamageType = DamageClass.Magic;
                // 可选：设置弹幕的额外属性
                Main.projectile[projectile].friendly = true;
                Main.projectile[projectile].hostile = false;
            }
        }
    }
}
