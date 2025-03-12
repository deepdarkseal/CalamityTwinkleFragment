using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Materials;


    namespace Calamitytwinklefragment.Content.Items.Accessories
    {
        public class TheAngerOfTheFarmer : ModItem
        {
            public override void SetDefaults()
            {
                Item.width = 38;  // 饰品宽度
                Item.height = 34; // 饰品高度
                Item.accessory = true; // 设置为饰品
                Item.rare = ItemRarityID.Red; // 稀有度
                Item.value = Item.buyPrice(0, 20, 50, 0); // 售价
            }

            public override void UpdateAccessory(Player player, bool hideVisual)
            {
                // 增加10%鞭子范围
                player.whipRangeMultiplier += 0.2f;

                // 增加60%鞭子伤害
                player.GetDamage(DamageClass.SummonMeleeSpeed) *= 1.6f;

                // 降低10%召唤物伤害
                player.GetDamage(DamageClass.Summon) *= 0.9f;

                // 增加10%召唤伤害
                player.GetDamage(DamageClass.Summon) += 0.10f;

                // 增加15鞭子攻速
                player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.16f; // 减少使用时间 = 增加攻速

                // 设置玩家标志，用于在鞭子攻击时触发减益
                player.GetModPlayer<WhipHookBladePlayer>().CuttingDebuff = true;
            }

            public override void AddRecipes()
            {
                CreateRecipe()
                    .AddIngredient<WeightedLeatherBall>() // 加重皮革球
                    .AddIngredient<WhipHookBlade>()  // 鞭钩刃
                    .AddIngredient(ModContent.ItemType < LifeAlloy>(), 20) //合金
                    .AddTile(TileID.MythrilAnvil)       // 在秘银砧或山铜砧上合成
                    .Register();
            }
        }
    }


