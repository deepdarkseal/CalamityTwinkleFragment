using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace Calamitytwinklefragment.Content.Items.Accessories.Mana
{
    class AstrumTelescope : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.buyPrice(0, 5, 0, 0); // 价格
            Item.rare = ItemRarityID.Blue; // 稀有度
            Item.accessory = true; // 设置为饰品
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<AstrumTelescopePlayer>().AstrumTelescopeEquipped = true;
            player.GetDamage<MagicDamageClass>() += 0.1f;
        }
    }
}
