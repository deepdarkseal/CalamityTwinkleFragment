using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;

namespace Calamitytwinklefragment.Content.Items.Fragment
{
    internal class FragmentTravelAdvertisement : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 添加提示行，告诉玩家按住左Shift查看更多信息
            TooltipLine shiftLine = new(Mod, "LegendaryItem100", "按住“左Shift”仔细聆听")
            {
                OverrideColor = Color.Yellow // 设置文本颜色
            };
            tooltips.Add(shiftLine);

            // 检测是否按下了左Shift键
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                // 添加第一行额外文本
                TooltipLine extraLine1 = new(Mod, "LegendaryItemExtra101", "伊尔梅里斯！海洋的璀璨明珠！风暴与魔法的交汇之地！")
                {
                    OverrideColor = Color.Yellow // 设置文本颜色
                };
                tooltips.Add(extraLine1);

                // 添加第二行额外文本
                TooltipLine extraLine2 = new(Mod, "LegendaryItemExtra102", "漫步于碧波环绕的珊瑚宫殿")
                {
                    OverrideColor = Color.Yellow
                };
                tooltips.Add(extraLine2);

                // 添加第三行额外文本
                TooltipLine extraLine3 = new(Mod, "LegendaryItemExtra103", "聆听海浪轻吟，感受神秘海风的轻抚。")
                {
                    OverrideColor = Color.Yellow
                };
                tooltips.Add(extraLine3);

                TooltipLine extraLine4 = new(Mod, "LegendaryItemExtra104", "期待您的探索与惊叹！")
                {
                    OverrideColor = Color.Yellow
                };
                tooltips.Add(extraLine4);

                TooltipLine extraLine5 = new(Mod, "LegendaryItemExtra105", "（配有一张很俗套的美人鱼插图）")
                {
                    OverrideColor = Color.Yellow
                };
                tooltips.Add(extraLine5);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.DesertChest).//沙岩箱
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
