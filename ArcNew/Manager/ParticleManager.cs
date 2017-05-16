using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.SDK.Renderer.Particle;
using log4net;
using PlaySharp.Toolkit.Logging;
using SharpDX;

namespace ArcAnnihilation.Manager
{
    public class ParticleManager : IParticleManager
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<ParticleEffectContainer> _particles = new List<ParticleEffectContainer>();

        private bool _disposed;

        public void AddOrUpdate(
            Entity unit,
            string name,
            string file,
            ParticleAttachment attachment,
            params object[] controlPoints)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (!unit.IsValid)
            {
                throw new ArgumentException("Value should be valid.", nameof(unit));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var particle = _particles.FirstOrDefault(p => p.Name == name);

            if (particle == null)
            {
                _particles.Add(new ParticleEffectContainer(name, file, unit, attachment, controlPoints));
            }
            else
            {
                // parts changed
                if (!ReferenceEquals(particle.Unit, unit) || particle.File != file || particle.Attachment != attachment)
                {
                    particle.Dispose();

                    _particles.Remove(particle);
                    _particles.Add(new ParticleEffectContainer(name, file, unit, attachment, controlPoints));
                    return;
                }

                // control points changed
                try
                {
                    var hash = controlPoints.Sum(p => p.GetHashCode());
                    if (particle.GetControlPointsHashCode() != hash)
                    {
                        particle.SetControlPoints(controlPoints);
                    }

                }
                catch (Exception)
                {

                }
                
            }
        }

        public void Dispose()
        {
            Printer.Both("disposed");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Draws a range indicator around the point
        /// </summary>
        /// <param name="center"></param>
        /// <param name="id"></param>
        /// <param name="range"></param>
        /// <param name="color"></param>
        public void DrawCircle(Vector3 center, string id, float range, Color color)
        {
            AddOrUpdate(
                ObjectManager.LocalHero,
                id,
                "particles/ui_mouseactions/drag_selected_ring.vpcf",
                ParticleAttachment.AbsOrigin,
                0,
                center,
                1,
                color,
                2,
                range * 1.1f);
        }

        /// <summary>
        /// Draws a red line from unit to endPosition
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="id"></param>
        /// <param name="endPosition"></param>
        public void DrawDangerLine(Unit unit, string id, Vector3 endPosition)
        {
            AddOrUpdate(
                unit,
                id,
                "particles/ui_mouseactions/range_finder_tower_line.vpcf",
                ParticleAttachment.AbsOriginFollow,
                6,
                true,
                2,
                endPosition,
                7,
                unit.Position);
        }

        /// <summary>
        /// Draws a line from unit to endPosition
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="id"></param>
        /// <param name="endPosition"></param>
        /// <param name="red"></param>
        public void DrawLine(Unit unit, string id, Vector3 endPosition, bool red = true)
        {
            var startPos = unit.Position;
            var pos1 = !red ? startPos : endPosition;

            AddOrUpdate(
                unit,
                id,
                "particles/ui_mouseactions/range_finder_line.vpcf",
                ParticleAttachment.AbsOrigin,
                0,
                startPos,
                1,
                pos1,
                2,
                endPosition);
        }

        /// <summary>
        /// Draws a range indicator around the unit
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="id"></param>
        /// <param name="range"></param>
        /// <param name="color"></param>
        public void DrawRange(Unit unit, string id, float range, Color color)
        {
            AddOrUpdate(
                unit,
                id,
                "particles/ui_mouseactions/drag_selected_ring.vpcf",
                ParticleAttachment.AbsOriginFollow,
                1,
                color,
                2,
                range * 1.1f);
        }

        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            foreach (var particle in _particles.Where(p => p.Name == name).ToArray())
            {
                _particles.Remove(particle);
                particle.Dispose();
            }
        }

        /// <summary>
        /// Shows the click effect on position
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void ShowClick(Unit unit, string id, Vector3 position, Color color)
        {
            AddOrUpdate(
                unit,
                id,
                "particles/ui_mouseactions/clicked_basemove.vpcf",
                ParticleAttachment.AbsOrigin,
                0,
                position,
                1,
                new Vector3(color.R, color.G, color.B));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var particle in _particles)
                {
                    try
                    {
                        particle?.Dispose();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            _disposed = true;
        }

        public bool HasParticle(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _particles.Any(p => p.Name == name);
        }
    }
}
