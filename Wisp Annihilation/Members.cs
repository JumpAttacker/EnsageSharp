using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;

namespace Wisp_Annihilation
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("Wisp Aniihilation", "WispAniihilation", true,
            "npc_dota_hero_wisp", true);

        public static Hero MyHero;
        public static Team MyTeam;

        public static List<string> Items;

        public static List<string> WhiteList = new List<string>
        {
            ""
        };
    }
}