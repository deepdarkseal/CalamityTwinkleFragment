using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories.Mana
{
    public class ManaBonsai : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 44;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 魔力消耗降低8%
            player.manaCost -= 0.08f;

            player.statManaMax2 += 50;
            // 自动使用魔力药水
            player.manaFlower = true;
  
            // 启用暴击率增加
            player.GetModPlayer<ManaBonsaiPlayer>().enableCritBonus = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ManaFlower) // 魔力花
                .AddIngredient(ModContent.ItemType< LivingShard > (),  8) // 生命碎片*8
                .AddTile(TileID.MythrilAnvil) // 合成站
                .Register();
        }
    }
}
