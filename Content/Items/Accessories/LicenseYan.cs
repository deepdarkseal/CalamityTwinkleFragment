using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Materials;
using static Terraria.ModLoader.ModContent;
using CalamityMod.Items;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Items.Accessories.DragonfireAmberPauldron;
using Calamitytwinklefragment.CTFplayer;

namespace Calamitytwinklefragment.Content.Items.Accessories
{
    class LicenseYan : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.accessory = true;
            Item.rare = RarityType<Turquoise>();
            Item.value = Item.buyPrice(0, 8, 22, 0); // 售价;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 激活自定义 ModPlayer 中的标识
            player.GetModPlayer<YanMiaoPowerPlayer>().hasLicenseYan = true;
            // 持续添加Buff（无论血量）
            player.AddBuff(BuffType<Buff.YanMiaoPower>(), 2); // 2帧持续，每帧刷新
            int damage = (int)(411 * player.GetDamage(DamageClass.Generic).ApplyTo(1f));
            // 召唤弹幕（仅在Buff存在时召唤）
            if (player.ownedProjectileCounts[ProjectileType<Projectiles.YanMiao>()] <= 0)
            {
                Projectile.NewProjectile(
                    player.GetSource_Accessory(Item),
                    player.Center,
                    Vector2.Zero,
                    ProjectileType<Projectiles.YanMiao>(),
                    damage, 0, player.whoAmI
                );
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LicenseCat).
                AddIngredient(ItemType<UelibloomBar> (), 99).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
