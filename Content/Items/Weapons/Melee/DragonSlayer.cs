using CalamityMod.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod;
using static CalamityMod.NPCs.BrimstoneElemental.BrimstoneElemental;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee
{
    class DragonSlayer : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.width = 160;
            Item.height = 160;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(0,3,6,5);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shootSpeed = 30f; // 弹幕速度
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            if (DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator)
            {
                Item.damage += 10;
            }
            if (DownedBossSystem.downedSlimeGod)
            {
                Item.damage += 10;
            }
            if (DownedBossSystem.downedSlimeGod)
            {
                Item.useTime = 35;
                Item.useAnimation = 35;
            }
            if (Main.hardMode)
            {
                Item.damage += 10;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.knockBack += 1;
            }
            if (DownedBossSystem.downedCryogen)
            {
                Item.useTime = 25;
                Item.useAnimation = 25;
            }
            if (DownedBossSystem.downedAquaticScourge)
            {
                Item.damage += 10;
            }
            if (DownedBossSystem.downedBrimstoneElemental)
            {
                crit += 10f; // 增加暴击率
            }
            if (DownedBossSystem.downedExoMechs)
            {
                crit += 10f; // 增加暴击率
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 99); //铁
            recipe.AddTile(TileID.Anvils);     // 合成站
            recipe.Register();
        }
    }
}
