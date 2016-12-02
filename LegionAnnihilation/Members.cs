using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace Legion_Annihilation
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("Legion Aniihilation", "LgAniihilation", true,
            "npc_dota_hero_legion_commander", true);

        public static Hero MyHero;
        public static Team MyTeam;
        public static ClassID MyClassId = ClassID.CDOTA_Unit_Hero_Legion_Commander;

        public static List<string> Items = new List<string>();

        public static List<string> AbilityList = new List<string>
        {
            "legion_commander_overwhelming_odds",
            "legion_commander_press_the_attack",
            "legion_commander_duel"
        };

        public static List<string> WhiteList = new List<string>
        {
            "item_urn_of_shadows",
            "item_armlet"
        };

        public static Sleeper Updater;
        public static List<string> BlackList = new List<string>
        {
            "item_blink"
        };
        public static ParticleEffect BlinkRange { get; set; }
    }
}
