using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Buff.Summom;
using Calamitytwinklefragment.Content.Projectiles.Summom;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Weapons.Summom
{
    internal class WeaverSong : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            // 可以添加工具提示等
        }

        public override void SetDefaults()
        {
            Item.damage = 904;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item44; // 召唤音效
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<WeaverMinionBuff>();
            Item.shoot = ModContent.ProjectileType<WeaverMinion>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 添加buff
            player.AddBuff(Item.buffType, 2);

            // 生成召唤物
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage/2, knockback, player.whoAmI);
            projectile.originalDamage = Item.damage/2;

            return false;
        }

        // 合成配方
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Cobweb, 20)
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}