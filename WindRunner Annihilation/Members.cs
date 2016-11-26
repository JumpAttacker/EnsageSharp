using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace WindRunner_Annihilation
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("WindRunner Aniihilation", "WrAniihilation", true,
            "npc_dota_hero_windrunner", true);

        public static Hero MyHero;
        public static Team MyTeam;
        public static ClassID MyClassId = ClassID.CDOTA_Unit_Hero_Windrunner;

        public static List<string> Items = new List<string>();

        public static List<string> AbilityList = new List<string>
        {
            "windrunner_shackleshot",
            "windrunner_powershot",
            "windrunner_windrun",
            "windrunner_focusfire",
        };

        public static List<string> WhiteList = new List<string>
        {
            "item_urn_of_shadows"
        };

        public static Sleeper Updater;
        public static List<string> BlackList = new List<string>
        {
            "item_blink"
        };
        public static ParticleEffect ShacklRange { get; set; }
        public static List<Vector3> BestPoinits { get; set; }
    }
}
