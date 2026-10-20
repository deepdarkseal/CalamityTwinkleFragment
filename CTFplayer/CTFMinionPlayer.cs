using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.CTFplayer
{
    internal class CTFMinionPlayer : ModPlayer
    {
        public bool weaverMinion = false;


        public override void ResetEffects()
        {
            weaverMinion = false;
        }
    }
}
