using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories
{
    internal class MagnetiteDice : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 31;
            Item.height = 32;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.value = Item.buyPrice(0, 7, 7, 7); // 售价;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 增加10点幸运
            player.luck += 10f;

            // 增加5%暴击率
            player.GetCritChance(DamageClass.Generic) += 5f;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OldDie> ().
                AddRecipeGroup("AnyMythrilBar", 5).// 任意秘银锭（秘银锭或山铜锭）
                AddTile(TileID.MythrilAnvil). // 在秘银砧上合成
                Register();
        }
    }
}
