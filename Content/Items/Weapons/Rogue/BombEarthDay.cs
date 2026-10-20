using CalamityMod.Items;
using Calamitytwinklefragment.Content.Items.Potions;
using CalamityMod.Rarities;
using CalamityMod;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Calamitytwinklefragment.Content.Projectiles;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Weapons.Rogue;

namespace Calamitytwinklefragment.Content.Items.Weapons.Rogue
{
    internal class BombEarthDay : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 0f;

            Item.value = Item.buyPrice(10,0,0,0);

            Item.rare = ModContent.RarityType<Turquoise>();
            Item.UseSound = SoundID.Item1;

            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<BombEarthDayProjectile>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj.WithinBounds(Main.maxProjectiles))
                Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SkyfinBombers>(1).//天鳍毁灭者
                AddIngredient(ItemID.Bomb,99).//炸弹
                AddIngredient(ItemID.Dynamite, 999).//雷管
                AddIngredient<WestLakeVinegarFish>(9999).//西湖醋鱼
                Register();
        }
    }
}
