using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;

namespace TemplarAnnihilation
{
    public class Members
    {
        public static readonly Menu Menu = new Menu("Templar Aniihilation", "TemplarAniihilation", true,
            "npc_dota_hero_templar_assassin", true);

        public static Hero MyHero;
        public static Team MyTeam;
        public static ClassId MyClassId = ClassId.CDOTA_Unit_Hero_TemplarAssassin;

        public static List<string> Items;

        public static ParticleEffect SpellRange;
    }
}
