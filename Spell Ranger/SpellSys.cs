using Ensage;

namespace SpellRanger
{
    internal class SpellSys
    {
        public Ability Spell;
        public bool Show;

        public SpellSys(Ability spell, bool show)
        {
            Spell = spell;
            Show = show;
        }

    }
}