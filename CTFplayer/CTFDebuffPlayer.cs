using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.CTFplayer
{
    internal class CTFDebuffPlayer : ModPlayer
    {
        public bool EternalDecayP = false;
        public override void ResetEffects()
        {
            EternalDecayP = false;
        }
        public override void NaturalLifeRegen(ref float regen)
        {

            if (EternalDecayP)
            {
                if (regen > 0)
                {
                    regen *= -3;
                }
                else
                {
                    regen *= 3;
                }
            }
        }
        
    }
}
