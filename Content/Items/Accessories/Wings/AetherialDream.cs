using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Items.Weapons.Mana.StardustLine;
using Calamitytwinklefragment.CTFplayer;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Items.Accessories.Wings
{
    [AutoloadEquip(EquipType.Wings)]
    internal class AetherialDream : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(3600, 15f, 5f);
        }
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = RarityType<HotPink>();
            Item.accessory = true;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 激活自定义 ModPlayer 中的标识
            player.GetModPlayer<AetherialDreamPlayer>().hasAetherialDream = true;
            player.wingTimeMax = 3600;
            player.wingAccRunSpeed = 15f;
            player.wingRunAccelerationMult = 5f;
            player.noFallDmg = true;
        }
        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1f;
            ascentWhenRising = 0.2f;
            maxCanAscendMultiplier = 1.7f;
            maxAscentMultiplier = 5f;
            constantAscend = 0.4f;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.CreativeWings).
                AddIngredient(ItemType<ShadowspecBar> (), 5).
                Register();
        }
    }
}
