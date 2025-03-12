using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calamitytwinklefragment.Content.Buff.Pets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Calamitytwinklefragment.Content.Projectiles.Pets.Shimu;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;

namespace Calamitytwinklefragment.Content.Items.Pets
{
    public class ShimuPet : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 36;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 1);

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item2;

            Item.shoot = ModContent.ProjectileType<ShimuPetProjectile>();
            Item.buffType = ModContent.BuffType<ShimuPetBuff>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }

        public override void AddRecipes()
        {
            // 合成配方（可根据需要调整）
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Biofusillade>()) // 生命光流
                .AddTile(TileID.Bookcases)            // 在书架上合成
                .Register();
        }
    }
}