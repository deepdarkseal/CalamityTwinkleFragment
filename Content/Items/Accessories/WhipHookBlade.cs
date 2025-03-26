using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories
{
    public class WhipHookBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;  // 饰品宽度
            Item.height = 26; // 饰品高度
            Item.accessory = true; // 设置为饰品
            Item.rare = ItemRarityID.LightRed; // 稀有度
            Item.value = Item.sellPrice(gold: 2); // 售价
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 增加10%召唤伤害
            player.GetDamage(DamageClass.Summon) += 0.10f;

            // 增加10%鞭子伤害
            player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.10f;

            // 设置玩家标志，用于在鞭子攻击时触发减益
            player.GetModPlayer<WhipHookBladePlayer>().CuttingDebuff = true;
        }

        public override void AddRecipes()
        {
            // 合成配方
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 5)       // 任意秘银锭（秘银锭或山铜锭）
                .AddIngredient(ItemID.SpiderFang, 10)    // 蜘蛛牙
                .AddIngredient(ItemID.SoulofNight, 5)  // 暗影之魂
                .AddTile(TileID.MythrilAnvil)       // 在秘银砧或山铜砧上合成
                .Register();
        }
    }
}