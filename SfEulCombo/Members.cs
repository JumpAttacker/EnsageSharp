using Ensage;
using Ensage.Common.Menu;

namespace SfEulCombo
{
    public static class Members
    {
        public static readonly Menu Menu = new Menu("Shadow Fiend Eul Combo", "SfEulCombo", true, "npc_dota_hero_nevermore", true);
        public static Hero MyHero;
        public static Hero Target;
        public static Ability Ultimate;
        public static Item Eul;
        public static Item Blink;
    }
}