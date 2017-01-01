using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;

namespace ModifierVision
{
    internal class Members
    {
        public static readonly Menu Menu = new Menu("Modifier Vision", "MV", true);
        public static Hero MyHero;
        public static List<HeroModifier> System;

        public static List<string> BlackList = new List<string>
        {
            "modifier_slark_essence_shift",
            "modifier_slark_essence_shift_buff",
            "modifier_slark_essence_shift_debuff",
            "modifier_slark_essence_shift_debuff_counter",
            "modifier_truesight",
            "modifier_shredder_reactive_armor_stack"
        };

        public static List<string> WhiteList = new List<string>
        {
            "modifier_nevermore_presence"
        };

        public static Dictionary<string, bool> ModiferDictinary = new Dictionary<string, bool>();
    }
}