using System.Reflection;
using Ensage;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    internal static class FireEvent
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                //Log.Info($"Event: {args.GameEvent.Name}");
            }
        }
    }
}