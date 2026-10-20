using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Calamitytwinklefragment.Content.Buff.Debuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Potions
{
    internal class Tofu : ModItem, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 50;
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = Item.buyPrice(0, 0, 0, 5);
            Item.rare = ItemRarityID.White;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.UseSound = SoundID.Item2;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useTurn = true;
            Item.buffType = BuffID.WellFed;
            Item.buffTime = CalamityUtils.SecondsToFrames(60f);
        }
        public override void OnConsumeItem(Player player)
        {
            // 添加减益效果
            player.AddBuff(ModContent.BuffType<EternalDecayDebuff>(), 3600); // 深渊水压，持续10分钟
            //player.AddBuff(ModContent.BuffType<RiptideDebuff>(), 3600);      // 激流，持续10分钟
            //player.AddBuff(ModContent.BuffType<Eutrophication>(), 3600);      // 富营养化，持续10分钟
            //player.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 3600);      // 硫磺海剧毒，持续10分钟
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).//水瓶
                Register();
        }
    }
}
