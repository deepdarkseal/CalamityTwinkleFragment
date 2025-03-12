using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Pets;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories.Mana
{
    public class StarFlameStone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Calamitytwinklefragment/Content/Items/Accessories/Mana/StarFlameStone";
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 7));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().ChaosStone = true;
            if (player.HasBuff(ModContent.BuffType<ManaBurn>()))
            {
                player.GetDamage<MagicDamageClass>() *= 1.10f; // 每个减益增加10%魔法伤害}
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ChaosStone>())  // 混乱石
                .AddIngredient(ModContent.ItemType<CoreofCalamity>()) // 核心
                .AddIngredient(ModContent.ItemType <AstralBar> (),9) //炫星锭*9
                .AddTile(TileID.MythrilAnvil) // 合成站
                .Register();
        }
    }
}
