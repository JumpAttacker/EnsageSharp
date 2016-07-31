using System.Collections.Generic;
using Ensage;

namespace ModifierVision
{
    internal class HeroModifier
    {
        public Unit Owner;
        public List<Modifier> Modifiers;
        public bool IsHero;
        public HeroModifier(Unit me, Modifier mod)
        {
            Owner = me;
            Modifiers = new List<Modifier> {mod};
            IsHero = me is Hero;
        }

        public void Add(Modifier mod)
        {
            Modifiers.Add(mod);
        }
        public void Remove(Modifier mod)
        {
            Modifiers.Remove(mod);
        }
    }
}