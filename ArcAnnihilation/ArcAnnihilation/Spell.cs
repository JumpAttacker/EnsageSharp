using System;
using Ensage;
using Ensage.Common.Objects;

namespace ArcAnnihilation
{
    public class Spell
    {
        private Ability spell;
        public string Name;
        private float _cd;
        private readonly DotaTexture _texture;

        public Spell(string name,float cooldown)
        {
            Name = name;
            _cd = cooldown;
            _texture = Textures.GetItemTexture(Name);
        }
        public Spell(Ability Ab)
        {
            Name = Ab.StoredName();
            spell = Ab;
            _cd = spell.Cooldown;
            _texture = Textures.GetItemTexture(Name);
        }

        public void SetCooldown(float cd)
        {
            _cd = cd;
        }
        public float GetCooldown()
        {
            return _cd;
        }
        public DotaTexture GetTexture()
        {
            return _texture;
        }

    }
}