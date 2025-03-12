using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Calamitytwinklefragment.Content.Items.Accessories
{
    public class WeightedLeatherBall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;  // 饰品宽度
            Item.height = 24; // 饰品高度
            Item.accessory = true; // 设置为饰品
            Item.rare = ItemRarityID.Green; // 稀有度
            Item.value = Item.buyPrice(0, 0, 70, 0); // 售价
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 增加10%鞭子范围
            player.whipRangeMultiplier += 0.2f;

            // 增加50%鞭子伤害
            player.GetDamage(DamageClass.SummonMeleeSpeed) *= 1.5f;

            // 降低10%召唤物伤害
            player.GetDamage(DamageClass.Summon) *= 0.9f;
        }

        public override void AddRecipes()
        {
            // 合成配方（可根据需要调整）
            CreateRecipe()
                .AddIngredient(ItemID.Leather, 10) // 皮革
                .AddRecipeGroup("AnyEvilBar", 5)  // 邪恶锭
                .AddRecipeGroup("Boss2Material", 10) //邪恶组织
                .AddTile(TileID.Anvils)            // 在铁砧上合成
                .Register();
        }
    }
}
