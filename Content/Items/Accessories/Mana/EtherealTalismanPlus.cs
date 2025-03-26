using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories.Mana
{
    public class EtherealTalismanPlus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<DarkBlue>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.eTalisman = true;

            // 启用暴击率增加
            player.GetModPlayer<ManaBonsaiPlayer>().enableCritBonus = true;

            player.manaMagnet = true;
            if (!hideVisual)
                player.manaFlower = true;

            player.statManaMax2 += 200;
            player.GetDamage<MagicDamageClass>() += 0.15f;
            player.manaCost *= 0.9f;
            player.GetCritChance<MagicDamageClass>() += 5;
        }
        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            // 禁止与 Community 或 EtherealTalisman 同时装备
            bool hasEtherealTalisman = false;

            // 遍历玩家的饰品栏
            for (int i = 3; i < 10; i++) // 饰品栏的索引是 3 到 9
            {
                Item accessory = player.armor[i];
                if (accessory.type == ModContent.ItemType<EtherealTalisman>())
                {
                    hasEtherealTalisman = true;
                    break;
                }
            }

            // 如果玩家已经装备了 Community 或 EtherealTalisman，则禁止装备 ShatteredCommunity
            return !hasEtherealTalisman;
        }

        public override bool CanUseItem(Player player) => false;
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ManaBonsai>().
                AddIngredient<SigilofCalamitas> ().
                AddIngredient(ItemID.LunarBar, 8).
                AddIngredient<GalacticaSingularity>(4).
                AddIngredient<AscendantSpiritEssence>(4).
                AddTile<CosmicAnvil>().
                Register();

            CreateRecipe().
                AddIngredient<EtherealTalisman>().
                AddIngredient(ModContent.ItemType<LivingShard>(), 8).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}