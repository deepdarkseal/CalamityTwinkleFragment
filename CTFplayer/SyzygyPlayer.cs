using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Calamitytwinklefragment.Content.Projectiles;
using Calamitytwinklefragment.Content.Items.Weapons.Ranged;

namespace Calamitytwinklefragment.CTFplayer
{
    public class SyzygyPlayer : ModPlayer
    {
        public int WangValue = 0;    // 望值
        public int ShuoValue = 0;    // 朔值
        public bool ReadyToReleaseWang = false;
        public bool ReadyToReleaseShuo = false;
        public int MoonProjectile = -1; // 月亮弹幕的索引

        public override void ResetEffects()
        {
            // 检查是否达到满月或朔月
            if (WangValue >= 14)
            {
                ReadyToReleaseWang = true;
            }
            else if (ShuoValue >= 14)
            {
                ReadyToReleaseShuo = true;
            }
        }

        public override void PostUpdate()
        {
            // 更新月亮弹幕的显示
            UpdateMoonDisplay();
        }

        private void UpdateMoonDisplay()
        {
            // 如果值大于4，显示月亮
            if (WangValue > 4 || ShuoValue > 4)
            {
                if (MoonProjectile == -1 || !Main.projectile[MoonProjectile].active || Main.projectile[MoonProjectile].type != ModContent.ProjectileType<SyzygyBowProjectile>())
                {
                    // 创建新的月亮弹幕
                    MoonProjectile = Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SyzygyBowProjectile>(),
                        0, 0, Player.whoAmI
                    );
                }

                // 更新月亮弹幕的数据
                if (MoonProjectile != -1 && Main.projectile[MoonProjectile].active)
                {
                    var proj = Main.projectile[MoonProjectile];
                    proj.ai[0] = WangValue;
                    proj.ai[1] = ShuoValue;
                }
            }
            else
            {
                // 值小于等于4，移除月亮弹幕
                if (MoonProjectile != -1 && Main.projectile[MoonProjectile].active)
                {
                    Main.projectile[MoonProjectile].Kill();
                    MoonProjectile = -1;
                }
            }
        }

        public override void OnConsumeAmmo(Item weapon, Item ammo)
        {
            // 朔望弓不消耗弹药
            if (weapon.type == ModContent.ItemType<SyzygyBow>())
            {
                // 这里可以设置消耗弹药的几率，设为0则不消耗
            }
        }
    }
}
