using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Items.Materials;
using Calamitytwinklefragment.Content.Projectiles;
using Calamitytwinklefragment.Content.Items.Accessories.DragonfireAmberPauldron;
using Terraria.DataStructures;
using CalamityMod.Projectiles.Melee;
using System.Security.Principal;
using CalamityMod.Rarities;
using CalamityMod.Items;

namespace Calamitytwinklefragment.Content.Items.Weapons.Mana
{
    internal class TerminalSound : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.damage = 1126; // 基础伤害值
            Item.knockBack = 5f;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.shootSpeed = 10f;
            Item.UseSound = SoundID.Item47; // 使用吉他斧的音效
            Item.autoReuse = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;

            // 设置默认弹幕类型
            Item.shoot = ProjectileID.FallingStar; // 默认弹幕类型
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 计算星星弹幕的伤害
            int starDamage = (int)(damage * 0.2f);

            // 获取鼠标位置相对于玩家的方向向量
            Vector2 mousePosition = Main.MouseWorld; // 鼠标的世界坐标
            Vector2 playerPosition = player.Center; // 玩家的中心坐标
            Vector2 direction = mousePosition - playerPosition; // 方向向量
            direction.Normalize(); // 归一化，使方向向量的长度为 1

            // 设置弹幕的速度
            float projectileSpeed = 21f;

            // 发射 7 枚星星弹幕，带有一定分散角度
            int starCount = 7; // 星星弹幕的数量
            float spreadAngle = MathHelper.ToRadians(7); // 分散角度（7 度）

            for (int i = 0; i < starCount; i++)
            {
                // 计算每枚星星弹幕的分散方向
                float angle = (i - (starCount - 1) / 2f) * spreadAngle; // 分散角度计算
                Vector2 starDirection = direction.RotatedBy(angle); // 分散后的方向
                Vector2 starSpeed = starDirection * projectileSpeed; // 分散后的速度

                // 发射星星弹幕 CosmicBolt GalaxiaBolt
                Projectile.NewProjectile(source, playerPosition, starSpeed, ModContent.ProjectileType<TerminalSoundStar>(), starDamage, knockback, player.whoAmI);
            }
            // 计算音波弹幕的伤害（基础伤害的170%）
            int soundWaveDamage = (int)(damage * 0.7f);
            // 发射音波弹幕
            Projectile.NewProjectile(source, playerPosition, Vector2.Zero, ModContent.ProjectileType<TerminalSoundWave>(), soundWaveDamage, knockback, player.whoAmI);

            return false; // 阻止默认的射击行为
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SparkleGuitar).//吉他
                AddIngredient(ModContent.ItemType<AshesofAnnihilation>(), 7).//湮灭余烬
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
