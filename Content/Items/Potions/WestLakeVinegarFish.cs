using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.TreasureBags.MiscGrabBags;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;

namespace Calamitytwinklefragment.Content.Items.Potions
{
    internal class WestLakeVinegarFish : ModItem, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 26;
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.UseSound = SoundID.Item2;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useTurn = true;
            Item.buffType = BuffID.WellFed2;
            Item.buffTime = CalamityUtils.SecondsToFrames(3600f);
        }
        public override void OnConsumeItem(Player player)
        {
            // 添加减益效果
            player.AddBuff(ModContent.BuffType<CrushDepth>(), 3600); // 深渊水压，持续10分钟
            player.AddBuff(ModContent.BuffType<RiptideDebuff>(), 3600);      // 激流，持续10分钟
            player.AddBuff(ModContent.BuffType<Eutrophication>(), 3600);      // 富营养化，持续10分钟
            player.AddBuff(ModContent.BuffType<SulphuricPoisoning> (), 3600);      // 硫磺海剧毒，持续10分钟
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater, 2).//水瓶
                AddIngredient(ItemID.Trout).
                AddTile(TileID.CookingPots).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater, 2).//水瓶
                AddIngredient(ItemID.Fish).
                AddTile(TileID.CookingPots).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater, 2).//水瓶
                AddIngredient(ItemID.Bass).
                AddTile(TileID.CookingPots).
                Register();
        }
    }
}
