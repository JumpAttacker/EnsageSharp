using System;
using Ensage;
using Ensage.Common.Objects;

namespace ArcAnnihilation
{
    public class Spell
    {
        public string Name;
        private float _cd;
        private readonly DotaTexture _texture;
        private float _lastTime;
        private float _lastCd;

        public Spell(string name,float cooldown)
        {
            Name = name;
            _cd = cooldown;
            _texture = Textures.GetItemTexture(Name);
        }
        public Spell(Ability ab)
        {
            Name = ab.StoredName();
            _cd = ab.Cooldown;
            _texture = Textures.GetItemTexture(Name);
        }

        public void SetCooldown(float cd)
        {
            _cd = cd;
        }

        public void Update(Ability ability)
        {
            _lastTime = Game.RawGameTime;
            _lastCd = ability.Cooldown;
        }
        public float GetLastTime()
        {
            return _lastTime;
        }
        public float GetLastCd()
        {
            return _lastCd;
        }
        public float GetCooldown()
        {
            //Console.WriteLine($"{Name}: cd: {this.GetLastCd() - Game.RawGameTime + this.GetLastTime() + 1}");
            return this.GetLastCd() - Game.RawGameTime + this.GetLastTime() + 1;
        }
        public DotaTexture GetTexture()
        {
            return _texture;
        }

    }
}