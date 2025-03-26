using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

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
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).//水瓶
                Register();
        }
    }
}
