using System;
using Ensage;
using Ensage.Common.Objects;

namespace ArcAnnihilation
{
    public class Spell
    {
        private Ability spell;
        public String Name;
        private float Cd;
        private DotaTexture texture;

        public Spell(string name,float cooldown)
        {
            Name = name;
            Cd = cooldown;
            texture = Textures.GetItemTexture(Name);
        }
        public Spell(Ability Ab)
        {
            Name = Ab.StoredName();
            spell = Ab;
            Cd = spell.Cooldown;
            texture = Textures.GetItemTexture(Name);
        }

        public void SetCooldown(float cd)
        {
            Cd = cd;
        }
        public float GetCooldown()
        {
            return Cd;
        }
        public DotaTexture GetTexture()
        {
            return texture;
        }

    }
}