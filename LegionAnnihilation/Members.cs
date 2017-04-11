using System.Collections.Generic;
using Ensage;
using Ensage.Common.Enums;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace Legion_Annihilation
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("Legion Aniihilation", "LgAniihilation", true,
            "npc_dota_hero_legion_commander", true);

        public static Hero MyHero;
        public static Team MyTeam;
        public static ClassId MyClassId = ClassId.CDOTA_Unit_Hero_Legion_Commander;

        public static List<string> Items = new List<string>();

        public static List<string> AbilityList = new List<string>
        {
            "legion_commander_overwhelming_odds",
            "legion_commander_press_the_attack",
            "legion_commander_duel"
        };

        public static List<ItemId> WhiteList = new List<ItemId>
        {
            ItemId.item_urn_of_shadows,
            ItemId.item_heavens_halberd,
            ItemId.item_armlet,
            ItemId.item_satanic,
            ItemId.item_force_staff,
            ItemId.item_hurricane_pike
        };

        public static Sleeper Updater;
        public static List<ItemId> BlackList = new List<ItemId>
        {
            ItemId.item_blink
        };
        public static ParticleEffect BlinkRange { get; set; }
    }
}
