using System.Reflection;
using Ensage;
using Ensage.Common;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace Auto_Disable
{
    internal class Program
    {
        private static bool _loaded;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static void Main()
        {
            MenuManager.Init();
            Events.OnLoad += (sender, eventArgs) =>
            {
                Log.Info("[Init] starting");
                if (!_loaded)
                {
                    Log.Info("MenuManager.Handle: start");
                    MenuManager.Handle();
                    Log.Info("MenuManager.Handle: end");
                    Members.MyTeam = ObjectManager.LocalPlayer.Team;
                    _loaded = true;
                    Game.PrintMessage($"AutoDisable loaded. ver {Assembly.GetExecutingAssembly().GetName().Version}");
                }
            };
            Events.OnClose += (sender, eventArgs) =>
            {
                if (_loaded)
                {
                    MenuManager.UnHandle();
                    _loaded = false;
                }
            };
            
        }
    }
}
