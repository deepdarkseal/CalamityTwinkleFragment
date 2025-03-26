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

namespace Calamitytwinklefragment.Content.Items.Fragment
{
    class FraBloodGodBattleRoar : ModItem
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
            Item.rare = ItemRarityID.Red;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 添加提示行，告诉玩家按住左Shift查看更多信息
            TooltipLine shiftLine = new(Mod, "LegendaryItem300", "按住“左Shift”仔细聆听")
            {
                OverrideColor = Color.Red // 设置文本颜色
            };
            tooltips.Add(shiftLine);

            // 检测是否按下了左Shift键
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                // 添加第一行额外文本
                TooltipLine extraLine1 = new(Mod, "LegendaryItemExtra301", "血神注视着我们！让敌人的鲜血染红大地！")
                {
                    OverrideColor = Color.Red // 设置文本颜色
                };
                tooltips.Add(extraLine1);

                // 添加第二行额外文本
                TooltipLine extraLine2 = new(Mod, "LegendaryItemExtra302", "颅骨归于血神，血肉归于尘土！杀！杀！杀！")
                {
                    OverrideColor = Color.Red
                };
                tooltips.Add(extraLine2);

                // 添加第三行额外文本
                TooltipLine extraLine3 = new(Mod, "LegendaryItemExtra303", "血祭血神，颅献颅座！今日便是敌人的末日！")
                {
                    OverrideColor = Color.Red
                };
                tooltips.Add(extraLine3);

                TooltipLine extraLine4 = new(Mod, "LegendaryItemExtra304", "血神的饥渴永不满足！让我们用鲜血填满祂的圣杯！")
                {
                    OverrideColor = Color.Red
                };
                tooltips.Add(extraLine4);

                TooltipLine extraLine5 = new(Mod, "LegendaryItemExtra305", "注：以上内容仅为参考，喊出大致意思就行了，不要背不出来在那想半天！")
                {
                    OverrideColor = Color.Red
                };
                tooltips.Add(extraLine5);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ModContent.ItemType<Bloodstone>()).//血石
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
