using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SfAnnihilation.Features;
using SharpDX;

namespace SfAnnihilation.DrawingStuff
{
    public class RazeRanger
    {
        private static readonly Vector3 EmptyColor = new Vector3(100, 100, 100);
        private static readonly Vector3 DetectedColor = new Vector3(0, 255, 0);
        public Ability Raze { get; set; }
        public ParticleEffect Effect { get; set; }
        public float Dist { get; set; }
        public RazeRanger(Ability a)
        {
            Raze = a;
            var range = a.GetRadius();
            Dist = a.CastRange;
            if (!MenuManager.DrawRazeRange)
                return;
            Effect = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", ObjectManager.LocalHero.Position);
            Effect.SetControlPoint(1, new Vector3(range, 255, 0));
            Effect.SetControlPoint(2, EmptyColor);
        }

        public bool IsValid => Dist > 0;

        public void UpdateRange()
        {
            Dist = Raze.CastRange;
        }
        public void UpdatePosition()
        {
            Effect.SetControlPoint(0, Prediction.InFront(Core.Me, Dist));
        }
        public void UpdateColors()
        {
            var targetSelection = Core.Target;
            if (targetSelection != null)
                Effect.SetControlPoint(2, Raze.CanHit(targetSelection) ? DetectedColor : EmptyColor);
            else
                Effect.SetControlPoint(2, EmptyColor);
        }

        public void Hide()
        {
            Effect.Dispose();
        }
        public void Show()
        {
            var range = Raze.GetRadius();
            Effect = new ParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf", ObjectManager.LocalHero.Position);
            Effect.SetControlPoint(1, new Vector3(range, 255, 0));
            Effect.SetControlPoint(2, EmptyColor);
        }
    }
    internal class RazeDrawing
    {
        private static readonly List<RazeRanger> RazeRangers = new List<RazeRanger>
        {
            new RazeRanger(Core.RazeLow),
            new RazeRanger(Core.RazeNormal),
            new RazeRanger(Core.RazeHigh)
        };
        public static void OnUpdate(EventArgs args)
        {
            if (!MenuManager.DrawRazeRange)
                return;
            foreach (var razeRanger in RazeRangers)
            {
                if (!razeRanger.IsValid)
                {
                    razeRanger.UpdateRange();
                    continue;
                }
                razeRanger.UpdatePosition();
                razeRanger.UpdateColors();
            }
        }

        public static void OnChange(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<bool>())
                Show();
            else
                Hide();
        }

        private static void Hide()
        {
            foreach (var razeRanger in RazeRangers)
                razeRanger.Hide();

        }

        private static void Show()
        {
            foreach (var razeRanger in RazeRangers)
                razeRanger.Show();
        }
    }
}