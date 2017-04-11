using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    
    internal static class RoshanAction
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Sleeper _sleeper;
        public static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (!Checker.IsActive()) return;

            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                Members.DeathTime = Game.GameTime;
                Members.RoshIsAlive = false;
            }
            if (args.GameEvent.Name == "aegis_event")
            {
                Members.AegisTime = Game.GameTime;
                Members.AegisEvent = true;
                Log.Info($"Event: {args.GameEvent.Name}");
            }
        }

        public static void Roshan(EventArgs args)
        {
            if (!Checker.IsActive()) return;
            if (!Members.Menu.Item("roshanTimer.Enable").GetValue<bool>() || _sleeper.Sleeping) return;
            _sleeper.Sleep(1000);
            var tickDelta = Game.GameTime - Members.DeathTime;
            Members.RoshanMinutes = Math.Floor(tickDelta / 60);
            Members.RoshanSeconds = tickDelta % 60;
            var roshan =
                ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(unit => unit.ClassId == ClassId.CDOTA_Unit_Roshan && unit.IsAlive);
            if (roshan != null)
            {
                Members.RoshIsAlive = true;
            }
            if (Members.AegisEvent)
            {
                tickDelta = Game.GameTime - Members.AegisTime;
                Members.AegisMinutes = Math.Floor(tickDelta/60);
                Members.AegisSeconds = tickDelta%60;
                //Log.Debug($"Timer {Members.AegisMinutes}:{Members.AegisSeconds}");
                if (!Members.AegisWasFound)
                    Members.Aegis = ObjectManager.GetEntities<Item>().FirstOrDefault(x => x.Name == "item_aegis");
                if (Members.Aegis != null && !Members.AegisWasFound)
                {
                    Log.Debug($"Aegis found! {Members.Aegis.Owner.StoredName()}");
                    Members.AegisWasFound = true;
                }
                if (4 - Members.AegisMinutes < 0 || (Members.AegisWasFound && (Members.Aegis==null || !Members.Aegis.IsValid)))
                {
                    Members.AegisEvent = false;
                    Members.AegisWasFound = false;
                    Log.Debug("Flush Aegis Timer");
                }
            }
        }
        public static void Flush()
        {
            _sleeper = new Sleeper();
        }
    }
}