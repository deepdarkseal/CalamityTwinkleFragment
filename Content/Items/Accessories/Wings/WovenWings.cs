using CalamityMod.Items.Materials;
using CalamityMod.Items;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Buffs.DamageOverTime;

namespace Calamitytwinklefragment.Content.Items.Accessories.Wings
{
    [AutoloadEquip(EquipType.Wings)]
    internal class WovenWings : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(240, 9f, 2.8f);
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 36;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.White;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.wingTimeMax = 240;
            player.wingAccRunSpeed = 9f;
            player.wingRunAccelerationMult = 2.8f;
            player.noFallDmg = true;
            player.buffImmune[ModContent.BuffType<Eutrophication>()] = true;
            player.buffImmune[ModContent.BuffType<GlacialState> ()] = true;
            player.buffImmune[BuffID.Webbed] = true;
            player.buffImmune[ModContent.BuffType<DoGExtremeGravity> ()] = true;
            player.buffImmune[ModContent.BuffType<IcarusFolly>()] = true;
            player.buffImmune[ModContent.BuffType<WeakPetrification> ()] = true;
            player.buffImmune[ModContent.BuffType<VulnerabilityHex> ()] = true;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 0.9f;
            ascentWhenRising = 0.16f;
            maxCanAscendMultiplier = 1.1f;
            maxAscentMultiplier = 3.2f;
            constantAscend = 0.12f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AerialiteBar>(5).
                AddIngredient(ModContent.ItemType<EffulgentFeather>(), 15). // 15个金羽
                AddIngredient(ItemID.SoulofFlight, 20).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
