using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Rarities;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Materials
{
    internal class MoonSilkSatin : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 10001;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(0, 6, 50, 0);
            Item.rare = ModContent.RarityType<Turquoise>();
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            float brightness = (float)Main.rand.Next(90, 111) * 0.01f;
            brightness *= Main.essScale;
            Lighting.AddLight((int)((Item.position.X + (float)(Item.width / 2)) / 16f), (int)((Item.position.Y + (float)(Item.height / 2)) / 16f), 0.6f * brightness, 0.6f * brightness, 0.8f * brightness);
        }
    }
}
