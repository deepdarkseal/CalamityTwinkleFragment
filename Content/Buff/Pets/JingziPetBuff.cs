using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Calamitytwinklefragment.Content.Projectiles.Pets.Shimu;
using Calamitytwinklefragment.Content.Projectiles.Pets.Jingzi;

namespace Calamitytwinklefragment.Content.Buff.Pets
{
    public class JingziPetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            int petProjectileType = ModContent.ProjectileType<JingziPetProjectile>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[petProjectileType] <= 0)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, petProjectileType, 0, 0f, player.whoAmI);
            }
        }
    }
}
