using CalamityMod;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Weapons.Ranged
{
    public class SyzygyBow : ModItem
    {
        // 月相数据
        public int WangValue = 0;    // 望值
        public int ShuoValue = 0;    // 朔值
        public bool ReadyToReleaseWang = false;
        public bool ReadyToReleaseShuo = false;
        public int MoonProjectile = -1; // 月亮弹幕的索引
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 300;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 160;
            Item.height = 160;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 40f;
            Item.useAmmo = AmmoID.Arrow;
        }
        public override void HoldItem(Player player)
        {
            // 更新月相状态
            UpdateMoonState();

            // 更新月亮弹幕的显示
            UpdateMoonDisplay(player);
        }

        private void UpdateMoonState()
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

        private void UpdateMoonDisplay(Player player)
        {
            // 如果值大于4，显示月亮
            if (WangValue > 4 || ShuoValue > 4)
            {
                if (MoonProjectile == -1 || !Main.projectile[MoonProjectile].active || Main.projectile[MoonProjectile].type != ModContent.ProjectileType<SyzygyBowProjectile>())
                {
                    // 创建新的月亮弹幕
                    MoonProjectile = Projectile.NewProjectile(
                        player.GetSource_FromThis(),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SyzygyBowProjectile>(),
                        0, 0, player.whoAmI
                    );
                }

                // 更新月亮弹幕的数据
                if (MoonProjectile != -1 && Main.projectile[MoonProjectile].active)
                {
                    var proj = Main.projectile[MoonProjectile];
                    proj.ai[0] = WangValue;
                    proj.ai[1] = ShuoValue;
                    // 更新位置跟随玩家
                    proj.Center = player.Center + new Vector2(0, -40);
                }
            }
            else
            {
                // 值小于等于4，移除月亮弹幕
                CleanupMoonProjectile();
            }
        }

        private void CleanupMoonProjectile()
        {
            if (MoonProjectile != -1 && Main.projectile[MoonProjectile].active)
            {
                Main.projectile[MoonProjectile].Kill();
                MoonProjectile = -1;
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键使用
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                ShuoValue = Math.Min(ShuoValue + 1, 14);
                WangValue = Math.Max(WangValue - 1, 0);
            }
            else // 左键
            {
                WangValue = Math.Min(WangValue + 1, 14);
                ShuoValue = Math.Max(ShuoValue - 1, 0);
            }
            if (player.altFunctionUse == 2f) // 右键攻击
            {
                Item.DamageType = DamageClass.Ranged;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noMelee = true;
                Item.knockBack = 4f;
                Item.autoReuse = true;
            }
            else // 左键攻击
            {
                Item.DamageType = DamageClass.Ranged;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noMelee = true;
                Item.knockBack = 4f;
                Item.autoReuse = true;
            }
            // 检查是否应该释放满月或朔月弹幕
            if (ReadyToReleaseWang)
            {
                ReleaseWangProjectile(player);
                WangValue = 0;
                ShuoValue = 0;
                ReadyToReleaseWang = false;
            }
            else if (ReadyToReleaseShuo)
            {
                ReleaseShuoProjectile(player);
                WangValue = 0;
                ShuoValue = 0;
                ReadyToReleaseShuo = false;
            }

            return true;
        }

        private void ReleaseWangProjectile(Player player)
        {
            // 计算射击方向
            Vector2 velocity = Main.MouseWorld - player.Center;
            velocity.Normalize();
            velocity *= Item.shootSpeed * 0.8f;

            // 释放满月弹幕
            _ = Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center,
                velocity,
                ModContent.ProjectileType<WangProjectile>(),
                (int)(Item.damage * 6f),
                Item.knockBack,
                player.whoAmI
            );
        }

        private void ReleaseShuoProjectile(Player player)
        {
            // 计算射击方向
            Vector2 velocity = Main.MouseWorld - player.Center;
            velocity.Normalize();
            velocity *= Item.shootSpeed * 1f;
            Main.NewText($"朔月", Color.Yellow);
            // 释放朔月弹幕
            _ = Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center,
                velocity,
                ModContent.ProjectileType<ShuoProjectile>(),
                (int)(Item.damage * 10f),
                Item.knockBack,
                player.whoAmI
            );
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 发射默认箭矢
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return false; // 阻止默认弹幕发射
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(2f, 0f);
        }
    }
}