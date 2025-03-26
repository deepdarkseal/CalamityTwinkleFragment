using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;

namespace Calamitytwinklefragment.Content.Items.Fragment
{
    class FraArmsOrder :ModItem
    {
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
            TooltipLine shiftLine = new(Mod, "LegendaryItem400", "按住“左Shift”仔细聆听")
            {
                OverrideColor = Color.DarkRed // 设置文本颜色
            };
            tooltips.Add(shiftLine);

            // 检测是否按下了左Shift键
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                // 添加第一行额外文本
                TooltipLine extraLine1 = new(Mod, "LegendaryItemExtra401", "订单编号：1803063009")
                {
                    OverrideColor = Color.DarkRed // 设置文本颜色
                };
                tooltips.Add(extraLine1);

                // 添加第二行额外文本
                TooltipLine extraLine2 = new(Mod, "LegendaryItemExtra402", "订单日期：1803年6月30号")
                {
                    OverrideColor = Color.DarkRed
                };
                tooltips.Add(extraLine2);

                // 添加第三行额外文本
                TooltipLine extraLine3 = new(Mod, "LegendaryItemExtra403", "甲方：玛那护教军第一方面军")
                {
                    OverrideColor = Color.DarkRed
                };
                tooltips.Add(extraLine3);

                TooltipLine extraLine4 = new(Mod, "LegendaryItemExtra404", "乙方：阿萨福勒第三护具厂")
                {
                    OverrideColor = Color.DarkRed
                };
                tooltips.Add(extraLine4);

                TooltipLine extraLine5 = new(Mod, "LegendaryItemExtra405", "需求：我们需要更强的防火涂层和附魔！！！")
                {
                    OverrideColor = Color.DarkRed
                };
                tooltips.Add(extraLine5);

                TooltipLine extraLine6 = new(Mod, "LegendaryItemExtra406", "那条该死的龙的吐息（后面的内容已经被硫磺火烧毁了）")
                {
                    OverrideColor = Color.DarkRed
                };
                tooltips.Add(extraLine6);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ModContent.ItemType<ScorchedBone> ()).//焦灼脊骨
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
