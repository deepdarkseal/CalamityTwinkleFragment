using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Melee;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items;

namespace Calamitytwinklefragment.Content.Items.Weapons.Ranged
{
    internal class Skyrend : ModItem, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Item.width = 50;  // 物品宽度
            Item.height = 142; // 物品高度
            Item.damage = 50; // 基础伤害
            Item.DamageType = DamageClass.Ranged; // 伤害类型为远程
            Item.useAnimation = 20; // 使用动画时长（帧）
            Item.useTime = 30;      // 实际使用间隔（帧）
            Item.useStyle = ItemUseStyleID.Shoot; // 使用动作（射击）
            Item.noMelee = true;    // 禁用近战攻击
            Item.knockBack = 2f;   // 击退强度
            Item.autoReuse = true; // 允许长按连续使用
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice; ; // 购买价格（5金币）
            Item.rare = ItemRarityID.LightRed; // 物品稀有度（浅红色）
            Item.shoot = ProjectileID.WoodenArrowFriendly; // 默认发射木箭
            Item.shootSpeed = 20f; // 弹幕初始速度
            Item.useAmmo = AmmoID.Arrow; // 使用箭作为弹药
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Galeforce>()) // 烈风
                .AddIngredient(ItemID.SoulofFlight, 10) // 飞翔之魂*10
                .AddRecipeGroup("AnyMythrilBar", 5) // 任意秘银锭（秘银锭或山铜锭）
                .AddTile(TileID.MythrilAnvil) // 在秘银砧上合成
                .Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 发射默认箭矢
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            // 播放音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item75, player.Center);
            // 召唤8束风暴从天而降
            for (int i = 0; i < 8; i++)
            {
                // 随机生成风暴的位置（玩家上方随机X，固定Y-600）
                Vector2 spawnPos = new(player.MountedCenter.X + Main.rand.Next(-400, 401), player.MountedCenter.Y - 600f);
                // 目标位置（鼠标位置+随机偏移）
                Vector2 targetPos = Main.MouseWorld + new Vector2(Main.rand.Next(-50, 51), Main.rand.Next(-50, 51));
                Vector2 stormVelocity = targetPos - spawnPos;
                stormVelocity.Normalize();
                stormVelocity *= 20f; // 速度

                // 发射风暴弹幕
                Projectile.NewProjectile(source, spawnPos, stormVelocity, ModContent.ProjectileType<StormBeam>(), (int)(damage * 0.3), knockback, player.whoAmI);
            }

            return false; // 阻止默认弹幕发射
        }
    }
}
