using System;
using System.Linq;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;

namespace OverlayInformation
{
    internal static class RoshanAction
    {
        private static Sleeper Sleeper;
        public static void Roshan(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (!Members.Menu.Item("roshanTimer.Enable").GetValue<bool>() || Sleeper.Sleeping) return;
            Sleeper.Sleep(1000);
            var tickDelta = Game.GameTime - Members.DeathTime;
            Members.RoshanMinutes = Math.Floor(tickDelta / 60);
            Members.RoshanSeconds = tickDelta % 60;
            var roshan =
                ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(unit => unit.ClassID == ClassID.CDOTA_Unit_Roshan && unit.IsAlive);
            if (roshan != null)
            {
                Members.RoshIsAlive = true;
            }
        }

        public static void Flush()
        {
            Sleeper = new Sleeper();
        }
    }
}