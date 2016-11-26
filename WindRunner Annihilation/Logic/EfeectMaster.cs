using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using SharpDX;

namespace WindRunner_Annihilation.Logic
{
    internal class LineHelpers
    {
        private static bool DrawLines => Members.Menu.Item("Range.Lines.Enable").GetValue<bool>();
        
        private static Dictionary<Entity, ParticleEffect> _effectDictinary;
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        public LineHelpers()
        {
            _effectDictinary = new Dictionary<Entity, ParticleEffect>();
            Game.OnUpdate += args =>
            {
                if (!IsEnable)
                    return;
                var freshDict = new List<Entity>();
                foreach (var track in _effectDictinary)
                {
                    var unit = track.Key;
                    var effect = track.Value;
                    if (ShackleshotCalculation.Target == null || !unit.IsAlive || unit.Distance2D(ShackleshotCalculation.Target) > 575 || !effect.IsValid || effect.IsDestroyed)
                    {
                        freshDict.Add(unit);
                    }
                    else
                    {
                        /*
                         *  1 Control Point (Position X, Y, Z)
                            2 Control Point (Position X, Y, Z)
                            3 Control Point (Alpha X), (Size Y), (MOD 2x Effect Z)
                            4 Control Point (Color X, Y, Z)
                         * */
                        effect.SetControlPoint(1, unit.Position);
                        effect.SetControlPoint(2, ShackleshotCalculation.Target.Position);
                    }
                }
                foreach (var unit in freshDict)
                {
                    Dispose(unit);
                }
            };
        }

        public void DrawEffect(Entity target, string effectName, bool someOne)
        {
            if (!DrawLines)
                return;
            ParticleEffect effect;
            if (!_effectDictinary.TryGetValue(target, out effect))
            {
                effect = new ParticleEffect(effectName, new Vector3());
                _effectDictinary.Add(target, effect);
                effect.SetControlPoint(3, new Vector3(255, 10, 0));
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

        public static void Dispose(Entity target)
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

    internal class DebugLines
    {
        private static Dictionary<Entity, ParticleEffect> _effectDictinary;
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        public DebugLines()
        {
            _effectDictinary = new Dictionary<Entity, ParticleEffect>();
            Game.OnUpdate += args =>
            {
                if (!IsEnable)
                    return;
                var freshDict = new List<Entity>();
                //var list = new List<Vector3>();
                foreach (var track in _effectDictinary)
                {
                    var unit = track.Key;
                    var effect = track.Value;
                    if (ShackleshotCalculation.Target == null || !unit.IsAlive || unit.Distance2D(ShackleshotCalculation.Target) > 575 || !effect.IsValid || effect.IsDestroyed)
                    {
                        freshDict.Add(unit);
                    }
                    else
                    {
                        /*
                         *  1 Control Point (Position X, Y, Z)
                            2 Control Point (Position X, Y, Z)
                            3 Control Point (Alpha X), (Size Y), (MOD 2x Effect Z)
                            4 Control Point (Color X, Y, Z)
                         * */
                        effect.SetControlPoint(2, ShackleshotCalculation.Target.Position);
                        var ang = ShackleshotCalculation.Target.FindAngleBetween(unit.Position, true);
                        var pos = ShackleshotCalculation.Target.Position -
                                  new Vector3((float)(800 * Math.Cos(ang)), (float)(800 * Math.Sin(ang)), 0);
                        effect.SetControlPoint(1, pos);
                        /*for (var i = 1; i < 16; i++)
                        {
                            var tempPos= ShackleshotCalculation.Target.Position -
                                  new Vector3((float)(i*50 * Math.Cos(ang)), (float)(i*50 * Math.Sin(ang)), 0);
                            if (NavMesh.GetCellFlags(tempPos) == NavMeshCellFlags.Walkable)
                                list.Add(tempPos);
                        }*/
                    }
                }
                //Members.BestPoinits = list;
                foreach (var unit in freshDict)
                {
                    Dispose(unit);
                }
            };
        }

        private static bool DrawDebugLines => Members.Menu.Item("Dev.DebugLines.enable").GetValue<bool>();
        public void DrawEffect(Entity target, string effectName, bool draw)
        {
            if (!DrawDebugLines)
                return;
            ParticleEffect effect;
            if (!_effectDictinary.TryGetValue(target, out effect))
            {
                if (draw)
                {
                    effect = new ParticleEffect(effectName, new Vector3());
                    _effectDictinary.Add(target, effect);
                    effect.SetControlPoint(3, new Vector3(255, 20, 0));
                }
            }
            else
            {
                if (effect != null && (!effect.IsValid || effect.IsDestroyed))
                {
                    Printer.Print("suka bleat");
                }
                else if (effect != null && (effect.IsValid && !draw))
                {
                    effect.Dispose();
                    return;
                }
            }
            effect?.SetControlPoint(4, (Vector3) Color.Purple);
        }

        public bool Dispose(Entity target)
        {
            ParticleEffect effect;
            if (!_effectDictinary.TryGetValue(target, out effect)) return false;
            effect.Dispose();
            _effectDictinary.Remove(target);
            var hero = target as Hero;
            Printer.Print(hero != null ? $"[Dispose]: {hero.GetRealName()}" : $"[Dispose]: {target.StoredName()}");
            return true;
        }
    }
}