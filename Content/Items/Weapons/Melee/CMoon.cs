using Calamitytwinklefragment.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee
{
    internal class CMoon : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            // 基础属性
            Item.damage = 650;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 64;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = false;
            Item.noUseGraphic = false;

            // 用于右键连枷模式的特殊设置
            Item.shoot = ModContent.ProjectileType<CMoonFlailProjectile>();
            Item.shootSpeed = 20f; // 修改为25
            Item.channel = true; // 允许持续使用
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键使用
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // 右键模式：连枷
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noMelee = true; // 不显示近战挥动
                Item.noUseGraphic = true; // 不显示物品图像
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.reuseDelay = 0;
                Item.autoReuse = false;

                // 设置不同的音效
                Item.UseSound = SoundID.Item19;
            }
            else
            {
                // 左键模式：正常挥砍
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noMelee = false;
                Item.noUseGraphic = false;
                Item.useTime = 25;
                Item.useAnimation = 25;
                Item.autoReuse = true;
                Item.UseSound = SoundID.Item1;
            }
            return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // 右键连枷模式 - 修改：向鼠标方向发射，速度25
                position = player.Center;

                // 计算玩家到鼠标的方向
                Vector2 direction = Main.MouseWorld - player.Center;
                direction.Normalize();

                // 设置速度方向向量
                velocity = direction * Item.shootSpeed;

                damage = Item.damage;
                type = ModContent.ProjectileType<CMoonFlailProjectile>();
            }
            else
            {
                // 左键挥砍模式
                position = player.Center;
                Vector2 direction = Main.MouseWorld - player.Center;
                direction.Normalize();
                velocity = direction * 25f;
                type = ModContent.ProjectileType<CMoonSwing>();
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // 检查是否已有连枷弹幕
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<CMoonFlailProjectile>())
                    {
                        // 如果已有连枷，让它立即开始返回而不是创建新的
                        proj.ai[0] = 1f; // 设置为返回状态
                        proj.netUpdate = true;
                        return false;
                    }
                }

                // 没有连枷时创建新的
                int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                Main.projectile[projIndex].originalDamage = Item.damage;
            }
            else
            {
                // 左键挥砍
                int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                Main.projectile[projIndex].rotation = velocity.ToRotation();
            }

            return false;
        }

        // 添加视觉特效
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // 左键挥砍时的月光特效
            if (player.altFunctionUse != 2)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(
                        new Vector2(hitbox.X, hitbox.Y),
                        hitbox.Width,
                        hitbox.Height,
                        DustID.Enchanted_Gold,
                        0f, 0f, 100, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity *= 0.5f;
                }
            }
        }
    }
}