using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Extensions;

namespace TinkerAnnihilation
{
    internal class ParticleEffectHelper
    {
        private readonly string _name;
        private readonly Dictionary<uint, ParticleEffect> _effectDictionary;

        public ParticleEffectHelper(string name)
        {
            _name = name;
            _effectDictionary=new Dictionary<uint, ParticleEffect>();
        }

        public void AddEffect(Unit target, ParticleEffect eff)
        {
            var handle = target.Handle;
            ParticleEffect effect;
            if (!_effectDictionary.TryGetValue(handle, out effect))
            {
                _effectDictionary.Add(handle, eff);
                Printer.Print($"[{_name}][NewEffect]: {target.Name}");
            }
            else
            {
                effect.Dispose();
                _effectDictionary.Remove(handle);
                AddEffect(target, eff);
                Printer.Print($"[{_name}][Remove&NewEffect]: {target.Name}");
            }
        }
        public void AddEffect(Unit target, ParticleEffect eff, float range)
        {
            var handle = target.Handle;
            ParticleEffect effect;
            if (!_effectDictionary.TryGetValue(handle, out effect))
            {
                _effectDictionary.Add(handle, eff);
                Printer.Print($"[{_name}][NewEffect]: {target.Name}");
                Game.OnUpdate += args =>
                {
                    if (!Game.IsInGame) return;
                    try
                    {
                        if (eff == null || !eff.IsValid || eff.IsDestroyed) return;
                        if (target.HasModifier("modifier_boots_of_travel_incoming"))
                        {
                            RemoveEffect(target);
                            return;
                        }
                        var frontPoint = Helper.InFront(Members.MyHero, target, range);
                        eff.SetControlPoint(1, target.Position);
                        eff.SetControlPoint(2, frontPoint);
                    }
                    catch (Exception)
                    {
                        
                    }
                    
                };
            }
            else
            {
                effect.Dispose();
                _effectDictionary.Remove(handle);
                AddEffect(target, eff);
                Printer.Print($"[{_name}][Remove&NewEffect]: {target.Name}");
            }
        }
        public void RemoveEffect(Unit target)
        {
            var handle = target.Handle;
            ParticleEffect effect;
            if (_effectDictionary.TryGetValue(handle, out effect))
            {
                if (effect.IsValid && !effect.IsDestroyed)
                    effect.Dispose();
                _effectDictionary.Remove(handle);
                Printer.Print($"[{_name}][RemoveEffect]: {target.Name}");
            }
        }
    }
}