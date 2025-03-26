using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Accessories;
using CalamityMod.Projectiles.Melee;
using CalamityMod;

namespace Calamitytwinklefragment.Content.Items.Accessories.DragonfireAmberPauldron // 替换为你的命名空间
{
    public class DragonfireAmberPauldron : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 56;
            Item.value = Item.buyPrice(0, 50, 0, 0); // 自定义价格
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 激活自定义 ModPlayer 中的标识
            player.GetModPlayer<DragonfirePauldronPlayer>().hasDragonfirePauldron = true;
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.lAmbergris = true;
            modPlayer.Pauldron = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SlagsplitterPauldron>()      // 熔火碎矿肩甲
                .AddIngredient<LeviathanAmbergris>()       // 利维坦龙涎香
                .AddIngredient(ModContent.ItemType<UnholyEssence> (), 25) // 25个精华
                .AddIngredient(ModContent.ItemType<ReaperTooth> (), 10) // 10个牙齿
                .AddTile(TileID.LunarCraftingStation)      // 合成站
                .Register();
        }
    }
}