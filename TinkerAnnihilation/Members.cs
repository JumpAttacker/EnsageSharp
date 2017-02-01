using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;

namespace TinkerAnnihilation
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("Tinker Aniihilation", "WispAniihilation", true,
            "npc_dota_hero_tinker", true);

        public static Hero MyHero;
        public static Team MyTeam;

        public static List<string> Items;
        public static List<string> AbilityList = new List<string>()
        {
            "tinker_laser",
            "tinker_heat_seeking_missile",
            /*"tinker_march_of_the_machines",*/
            "tinker_rearm"
        };
        public static List<string> UsagesList = new List<string>()
        {
            "tinker_laser",
            "tinker_heat_seeking_missile",
            /*"tinker_march_of_the_machines",*/
        };

        public static List<string> WhiteList=new List<string>
        {
            "item_ghost"
        };

        internal static ParticleEffectHelper TowerRangEffectHelper;
        internal static ParticleEffectHelper TowerRangEffectHelper2;

        public static int TargetR => Menu.Item("Target.Red").GetValue<Slider>().Value;
        public static int TargetG => Menu.Item("Target.Green").GetValue<Slider>().Value;
        public static int TargetB => Menu.Item("Target.Blue").GetValue<Slider>().Value;


        public static bool LaserBuff;
    }
}