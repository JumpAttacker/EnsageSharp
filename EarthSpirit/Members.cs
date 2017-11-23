using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;

namespace EarthAn
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("Earth Aniihilation", "EarthAniihilation", true,
            "npc_dota_hero_earth_spirit", true);

        public static Hero MyHero;
        public static Team MyTeam;
        public static HeroId MyHeroId = HeroId.npc_dota_hero_earth_spirit;

        public static List<string> Items;

        public static List<string> AbilityList = new List<string>()
        {
            "earth_spirit_magnetize",
            "earth_spirit_petrify",
            "earth_spirit_stone_caller",
            "earth_spirit_geomagnetic_grip",
            "earth_spirit_rolling_boulder",
            "earth_spirit_boulder_smash"
        };

        public static ParticleEffect SpellRange;
    }
}
