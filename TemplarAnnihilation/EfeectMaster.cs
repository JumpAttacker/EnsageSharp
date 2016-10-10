using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using SharpDX;

namespace TemplarAnnihilation
{
    internal class EfeectMaster
    {
        private static Dictionary<Unit, ParticleEffect> _effectDictinary;
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        public EfeectMaster()
        {
            _effectDictinary = new Dictionary<Unit, ParticleEffect>();
            Game.OnUpdate += args =>
            {
                if (!IsEnable)
                    return;
                var freshDict = new List<Unit>();
                foreach (var track in _effectDictinary)
                {
                    var unit = track.Key;
                    var effect = track.Value;
                    if (!unit.IsAlive || !unit.IsVisibleToEnemies || unit.Distance2D(Members.MyHero) > Members.MyHero.GetAttackRange()+ unit.HullRadius || !effect.IsValid || effect.IsDestroyed)
                    {
                        freshDict.Add(unit);
                    }
                    else
                    {
                        var psiBlade = Abilities.FindAbility("templar_assassin_psi_blades");
                        var extraRange = 550 + 40 * psiBlade.Level;
                        var heroPos = unit.Position;
                        var angle = Members.MyHero.FindAngleBetween(heroPos, true);
                        var point = new Vector3(
                            (float)
                                (heroPos.X +
                                 extraRange *
                                 Math.Cos(angle)),
                            (float)
                                (heroPos.Y +
                                 extraRange *
                                 Math.Sin(angle)),
                            0);
                        effect.SetControlPoint(1, heroPos);
                        effect.SetControlPoint(2, point);
                    }
                }
                foreach (var unit in freshDict)
                {
                    Dispose(unit);
                }
            };
        }

        public void DrawEffect(Unit target, string effectName, bool someOne)
        {
            ParticleEffect effect;
            if (!_effectDictinary.TryGetValue(target, out effect))
            {
                effect = new ParticleEffect(effectName, new Vector3());
                effect.SetControlPoint(3, new Vector3(75, 140, 0));
                _effectDictinary.Add(target, effect);
            }
            else
            {
                if (!effect.IsValid || effect.IsDestroyed)
                {
                    Printer.Print("suka bleat");
                }
                //effect.Dispose();
            }
            effect.SetControlPoint(4, new Vector3(someOne ? 0 : 255, someOne ? 255 : 0, 0));
        }

        public static void Dispose(Unit target)
        {
            ParticleEffect effect;
            if (_effectDictinary.TryGetValue(target, out effect))
            {
                effect.Dispose();
                _effectDictinary.Remove(target);
                var hero = target as Hero;
                Printer.Print(hero != null ? $"[Dispose]: {hero.GetRealName()}" : $"[Dispose]: {target.StoredName()}");
            }
        }
    }
}