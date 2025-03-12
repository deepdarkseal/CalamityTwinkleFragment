using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Calamitytwinklefragment.Content.Items.Fragment
{
    class FraMoldyPaper : ModItem
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
            Item.rare = ItemRarityID.Blue;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 添加提示行，告诉玩家按住左Shift查看更多信息
            TooltipLine shiftLine = new(Mod, "LegendaryItem100", "按住“左Shift”仔细聆听")
            {
                OverrideColor = Color.LightSteelBlue // 设置文本颜色
            };
            tooltips.Add(shiftLine);

            // 检测是否按下了左Shift键
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                // 添加第一行额外文本
                TooltipLine extraLine1 = new(Mod, "LegendaryItemExtra201", "《Symbiotic Relationship Between Hermititan and Blue Fluorescent Fungi: ")
                {
                    OverrideColor = Color.LightSteelBlue // 设置文本颜色
                };
                tooltips.Add(extraLine1);

                // 添加第二行额外文本
                TooltipLine extraLine2 = new(Mod, "LegendaryItemExtra202", "Ecological, Behavioral, and Mechanistic Insights》")
                {
                    OverrideColor = Color.LightSteelBlue
                };
                tooltips.Add(extraLine2);

                // 添加第三行额外文本
                TooltipLine extraLine3 = new(Mod, "LegendaryItemExtra203", "研究对象：Hermititan（巴罗蟹）、蓝色荧光真菌")
                {
                    OverrideColor = Color.LightSteelBlue
                };
                tooltips.Add(extraLine3);

                TooltipLine extraLine4 = new(Mod, "LegendaryItemExtra204", "研究内容：探讨Hermititan与蓝色荧光真菌真菌的共生习性，包括生态学意义、行为学表现及共生机制。")
                {
                    OverrideColor = Color.LightSteelBlue
                };
                tooltips.Add(extraLine4);

                TooltipLine extraLine5 = new(Mod, "LegendaryItemExtra205", "研究目标：揭示这种共生关系对Hermititan生存、繁殖及生态系统功能的影响。")
                {
                    OverrideColor = Color.LightSteelBlue
                };
                tooltips.Add(extraLine5);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.GlowingMushroom,20).//蘑菇
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
