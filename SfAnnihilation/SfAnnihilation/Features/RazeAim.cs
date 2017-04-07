using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Utils;
using SharpDX;

namespace SfAnnihilation.Features
{
    internal class RazeAim
    {
        public static readonly Hero Me = ObjectManager.LocalHero;
        public static readonly Sleeper KillStealer = new Sleeper();
        public static readonly Sleeper Updater = new Sleeper();
        public static void OnUpdate(EventArgs args)
        {
            if (!MenuManager.AimIsActive || Updater.Sleeping || MenuManager.ComboIsActive)
                return;
            var target =
                Heroes.GetByTeam(Me.GetEnemyTeam())
                    .FirstOrDefault(
                        x =>
                            x.IsAlive && x.IsVisible && x.IsValidRazeTarget() &&
                            (!MenuManager.AimKillStealOnly ||
                             AbilityDamage.CalculateDamage(Core.RazeLow, Core.Me, x) > x.Health + x.HealthRegeneration));
            if (target != null)
            {
                if (!KillStealer.Sleeping && Core.Razes.Any(x=>x.CanBeCasted() && x.CanHit(target,checkForFace:false)) && !Core.Razes.Any(y => y.IsInAbilityPhase))
                {
                    var mePos = Me.Position;
                    var targetPos = Prediction.PredictedXYZ(target, 550);//target.Position;
                    var angle = Me.FindAngleBetween(targetPos, true);
                    var point = new Vector3(
                        (float)
                            (mePos.X +
                             100 *
                             Math.Cos(angle)),
                        (float)
                            (mePos.Y +
                             100 *
                             Math.Sin(angle)),
                        target.Position.Z);
                    Me.Move(point);
                    KillStealer.Sleep(350);
                }
                if (Helper.RazeAimCaster(Core.RazeLow, target))
                    Updater.Sleep(1000);
                else if (Helper.RazeAimCaster(Core.RazeNormal, target))
                    Updater.Sleep(1000);
                else if (Helper.RazeAimCaster(Core.RazeHigh, target))
                    Updater.Sleep(1000);
            }
        }
    }
}